﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using AuthJanitor.UI.Shared.MetaServices;
using AuthJanitor.UI.Shared.Models;
using AuthJanitor.UI.Shared.ViewModels;
using AuthJanitor.EventSinks;
using AuthJanitor.IdentityServices;
using AuthJanitor.Integrations.DataStores;
using AuthJanitor.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http;

namespace AuthJanitor
{
    /// <summary>
    /// API functions to control the creation and management of AuthJanitor Resources.
    /// A Resource is the description of how to connect to an object or resource, using a given Provider.
    /// </summary>
    public class Resources
    {
        private readonly IIdentityService _identityService;
        private readonly ProviderManagerService _providerManager;
        private readonly EventDispatcherMetaService _eventDispatcher;

        private readonly IDataStore<Resource> _resources;
        private readonly Func<Resource, ResourceViewModel> _resourceViewModel;

        public Resources(
            IIdentityService identityService,
            EventDispatcherMetaService eventDispatcher,
            ProviderManagerService providerManager,
            IDataStore<Resource> resourceStore,
            Func<Resource, ResourceViewModel> resourceViewModelDelegate)
        {
            _identityService = identityService;
            _eventDispatcher = eventDispatcher;
            _providerManager = providerManager;

            _resources = resourceStore;
            _resourceViewModel = resourceViewModelDelegate;
        }

        [FunctionName("Resources-Create")]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "resources")] ResourceViewModel resource,
            HttpRequest req)
        {
            _ = req;

            if (!_identityService.CurrentUserHasRole(AuthJanitorRoles.ResourceAdmin)) return new UnauthorizedResult();

            var provider = _providerManager.GetProviderMetadata(resource.ProviderType);
            if (provider == null)
                return new NotFoundObjectResult("Provider type not found");

            if (!_providerManager.TestProviderConfiguration(provider.ProviderTypeName, resource.SerializedProviderConfiguration))
                return new BadRequestErrorMessageResult("Invalid Provider configuration!");

            Resource newResource = new Resource()
            {
                Name = resource.Name,
                Description = resource.Description,
                IsRekeyableObjectProvider = provider.IsRekeyableObjectProvider,
                ProviderType = provider.ProviderTypeName,
                ProviderConfiguration = resource.SerializedProviderConfiguration
            };

            await _resources.Create(newResource);

            await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.ResourceCreated, nameof(Resources.Create), newResource);

            return new OkObjectResult(_resourceViewModel(newResource));
        }

        [FunctionName("Resources-List")]
        public async Task<IActionResult> List([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "resources")] HttpRequest req)
        {
            _ = req;

            if (!_identityService.IsUserLoggedIn) return new UnauthorizedResult();

            return new OkObjectResult((await _resources.Get()).Select(r => _resourceViewModel(r)));
        }

        [FunctionName("Resources-Get")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "resources/{resourceId:guid}")] HttpRequest req,
            Guid resourceId)
        {
            _ = req;

            if (!_identityService.IsUserLoggedIn) return new UnauthorizedResult();

            if (!await _resources.ContainsId(resourceId))
            {
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(Resources.Get), "Resource not found");
                return new NotFoundResult();
            }

            return new OkObjectResult(_resourceViewModel(await _resources.GetOne(resourceId)));
        }

        [FunctionName("Resources-Delete")]
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "resources/{resourceId:guid}")] HttpRequest req,
            Guid resourceId)
        {
            _ = req;

            if (!_identityService.CurrentUserHasRole(AuthJanitorRoles.ResourceAdmin)) return new UnauthorizedResult();

            if (!await _resources.ContainsId(resourceId))
            {
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(Resources.Delete), "Resource not found");
                return new NotFoundResult();
            }

            await _resources.Delete(resourceId);

            await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.ResourceDeleted, nameof(Resources.Delete), resourceId);

            return new OkResult();
        }

        [FunctionName("Resources-Update")]
        public async Task<IActionResult> Update(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "resources/{resourceId:guid}")] ResourceViewModel resource,
            HttpRequest req,
            Guid resourceId)
        {
            _ = req;

            if (!_identityService.CurrentUserHasRole(AuthJanitorRoles.ResourceAdmin)) return new UnauthorizedResult();

            var provider = _providerManager.GetProviderMetadata(resource.ProviderType);
            if (provider == null)
                return new NotFoundObjectResult("Provider type not found");

            if (!_providerManager.TestProviderConfiguration(provider.ProviderTypeName, resource.SerializedProviderConfiguration))
                return new BadRequestErrorMessageResult("Invalid Provider configuration!");

            Resource newResource = new Resource()
            {
                ObjectId = resourceId,
                Name = resource.Name,
                Description = resource.Description,
                IsRekeyableObjectProvider = resource.IsRekeyableObjectProvider,
                ProviderType = resource.ProviderType,
                ProviderConfiguration = resource.SerializedProviderConfiguration
            };

            await _resources.Update(newResource);

            await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.ResourceUpdated, nameof(Resources.Update), newResource);

            return new OkObjectResult(_resourceViewModel(newResource));
        }
    }
}

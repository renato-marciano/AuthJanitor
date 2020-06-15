// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
<<<<<<< HEAD:AuthJanitor.Functions.AdminApi/ManagedSecrets.cs
using AuthJanitor.UI.Shared.MetaServices;
using AuthJanitor.UI.Shared.Models;
using AuthJanitor.UI.Shared.ViewModels;
=======
>>>>>>> master:AuthJanitor.Functions.AdminApi/Services/ManagedSecretsService.cs
using AuthJanitor.CryptographicImplementations;
using AuthJanitor.EventSinks;
using AuthJanitor.IdentityServices;
using AuthJanitor.Integrations.DataStores;
using AuthJanitor.Providers;
using AuthJanitor.UI.Shared.MetaServices;
using AuthJanitor.UI.Shared.Models;
using AuthJanitor.UI.Shared.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

<<<<<<< HEAD:AuthJanitor.Functions.AdminApi/ManagedSecrets.cs
namespace AuthJanitor
=======
namespace AuthJanitor.Services
>>>>>>> master:AuthJanitor.Functions.AdminApi/Services/ManagedSecretsService.cs
{
    /// <summary>
    /// API functions to control the creation and management of AuthJanitor Managed Secrets.
    /// A Managed Secret is a grouping of Resources and Policies which describe the strategy around rekeying an object and the applications which consume it.
    /// </summary>
    public class ManagedSecretsService
    {
        private readonly AuthJanitorCoreConfiguration _configuration;
        private readonly IIdentityService _identityService;
        private readonly ICryptographicImplementation _cryptographicImplementation;
        private readonly ProviderManagerService _providerManager;
        private readonly EventDispatcherMetaService _eventDispatcher;

        private readonly IDataStore<ManagedSecret> _managedSecrets;
        private readonly IDataStore<Resource> _resources;
        private readonly Func<ManagedSecret, ManagedSecretViewModel> _managedSecretViewModel;

        public ManagedSecretsService(
            IOptions<AuthJanitorCoreConfiguration> configuration,
            IIdentityService identityService,
            ICryptographicImplementation cryptographicImplementation,
            EventDispatcherMetaService eventDispatcher,
            ProviderManagerService providerManager,
            IDataStore<ManagedSecret> managedSecretStore,
            IDataStore<Resource> resourceStore,
            Func<ManagedSecret, ManagedSecretViewModel> managedSecretViewModelDelegate)
        {
            _configuration = configuration.Value;
            _identityService = identityService;
            _cryptographicImplementation = cryptographicImplementation;
            _eventDispatcher = eventDispatcher;
            _providerManager = providerManager;

            _managedSecrets = managedSecretStore;
            _resources = resourceStore;
            _managedSecretViewModel = managedSecretViewModelDelegate;
        }

        public async Task<IActionResult> Create(ManagedSecretViewModel inputSecret)
        {
            if (!_identityService.CurrentUserHasRole(AuthJanitorRoles.SecretAdmin)) return new UnauthorizedResult();

            var resources = await _resources.Get();
            var resourceIds = inputSecret.ResourceIds.Split(';').Select(r => Guid.Parse(r)).ToList();
            if (resourceIds.Any(id => !resources.Any(r => r.ObjectId == id)))
            {
<<<<<<< HEAD:AuthJanitor.Functions.AdminApi/ManagedSecrets.cs
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(ManagedSecrets.Create), "New Managed Secret attempted to use one or more invalid Resource IDs");
=======
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(ManagedSecretsService.Create), "New Managed Secret attempted to use one or more invalid Resource IDs");
>>>>>>> master:AuthJanitor.Functions.AdminApi/Services/ManagedSecretsService.cs
                return new NotFoundObjectResult("One or more Resource IDs not found!");
            }

            ManagedSecret newManagedSecret = new ManagedSecret()
            {
                Name = inputSecret.Name,
                Description = inputSecret.Description,
                ValidPeriod = TimeSpan.FromMinutes(inputSecret.ValidPeriodMinutes),
                LastChanged = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(inputSecret.ValidPeriodMinutes),
                TaskConfirmationStrategies = inputSecret.TaskConfirmationStrategies,
                ResourceIds = resourceIds,
                Nonce = await _cryptographicImplementation.GenerateCryptographicallySecureString(_configuration.DefaultNonceLength)
            };

            await _managedSecrets.Create(newManagedSecret);

<<<<<<< HEAD:AuthJanitor.Functions.AdminApi/ManagedSecrets.cs
            await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.SecretCreated, nameof(ManagedSecrets.Create), newManagedSecret);
=======
            await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.SecretCreated, nameof(ManagedSecretsService.Create), newManagedSecret);
>>>>>>> master:AuthJanitor.Functions.AdminApi/Services/ManagedSecretsService.cs

            return new OkObjectResult(_managedSecretViewModel(newManagedSecret));
        }

        public async Task<IActionResult> List(HttpRequest req)
        {
            _ = req;

            if (!_identityService.IsUserLoggedIn) return new UnauthorizedResult();

            return new OkObjectResult((await _managedSecrets.Get()).Select(s => _managedSecretViewModel(s)));
        }

        public async Task<IActionResult> Get(HttpRequest req, Guid secretId)
        {
            _ = req;

            if (!_identityService.IsUserLoggedIn) return new UnauthorizedResult();

            if (!await _managedSecrets.ContainsId(secretId))
            {
<<<<<<< HEAD:AuthJanitor.Functions.AdminApi/ManagedSecrets.cs
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(ManagedSecrets.Get), "Secret ID not found");
=======
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(ManagedSecretsService.Get), "Secret ID not found");
>>>>>>> master:AuthJanitor.Functions.AdminApi/Services/ManagedSecretsService.cs
                return new NotFoundObjectResult("Secret not found!");
            }

            return new OkObjectResult(_managedSecretViewModel(await _managedSecrets.GetOne(secretId)));
        }

        public async Task<IActionResult> Delete(HttpRequest req, Guid secretId)
        {
            _ = req;

            if (!_identityService.CurrentUserHasRole(AuthJanitorRoles.SecretAdmin)) return new UnauthorizedResult();

            if (!await _managedSecrets.ContainsId(secretId))
            {
<<<<<<< HEAD:AuthJanitor.Functions.AdminApi/ManagedSecrets.cs
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(ManagedSecrets.Delete), "Secret ID not found");
=======
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(ManagedSecretsService.Delete), "Secret ID not found");
>>>>>>> master:AuthJanitor.Functions.AdminApi/Services/ManagedSecretsService.cs
                return new NotFoundObjectResult("Secret not found!");
            }

            await _managedSecrets.Delete(secretId);

<<<<<<< HEAD:AuthJanitor.Functions.AdminApi/ManagedSecrets.cs
            await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.SecretDeleted, nameof(ManagedSecrets.Delete), secretId);
=======
            await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.SecretDeleted, nameof(ManagedSecretsService.Delete), secretId);
>>>>>>> master:AuthJanitor.Functions.AdminApi/Services/ManagedSecretsService.cs

            return new OkResult();
        }

        public async Task<IActionResult> Update(ManagedSecretViewModel inputSecret, Guid secretId)
        {
            if (!_identityService.CurrentUserHasRole(AuthJanitorRoles.SecretAdmin)) return new UnauthorizedResult();

            if (!await _managedSecrets.ContainsId(secretId))
            {
<<<<<<< HEAD:AuthJanitor.Functions.AdminApi/ManagedSecrets.cs
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(ManagedSecrets.Update), "Secret ID not found");
=======
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(ManagedSecretsService.Update), "Secret ID not found");
>>>>>>> master:AuthJanitor.Functions.AdminApi/Services/ManagedSecretsService.cs
                return new NotFoundObjectResult("Secret not found!");
            }

            var resources = await _resources.Get();
            var resourceIds = inputSecret.ResourceIds.Split(';').Select(r => Guid.Parse(r)).ToList();
            if (resourceIds.Any(id => !resources.Any(r => r.ObjectId == id)))
            {
<<<<<<< HEAD:AuthJanitor.Functions.AdminApi/ManagedSecrets.cs
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(ManagedSecrets.Update), "New Managed Secret attempted to use one or more invalid Resource IDs");
=======
                await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.AnomalousEventOccurred, nameof(ManagedSecretsService.Update), "New Managed Secret attempted to use one or more invalid Resource IDs");
>>>>>>> master:AuthJanitor.Functions.AdminApi/Services/ManagedSecretsService.cs
                return new NotFoundObjectResult("One or more Resource IDs not found!");
            }

            ManagedSecret newManagedSecret = new ManagedSecret()
            {
                ObjectId = secretId,
                Name = inputSecret.Name,
                Description = inputSecret.Description,
                ValidPeriod = TimeSpan.FromMinutes(inputSecret.ValidPeriodMinutes),
                TaskConfirmationStrategies = inputSecret.TaskConfirmationStrategies,
                ResourceIds = resourceIds,
                Nonce = await _cryptographicImplementation.GenerateCryptographicallySecureString(_configuration.DefaultNonceLength)
            };

            await _managedSecrets.Update(newManagedSecret);

<<<<<<< HEAD:AuthJanitor.Functions.AdminApi/ManagedSecrets.cs
            await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.SecretUpdated, nameof(ManagedSecrets.Update), newManagedSecret);
=======
            await _eventDispatcher.DispatchEvent(AuthJanitorSystemEvents.SecretUpdated, nameof(ManagedSecretsService.Update), newManagedSecret);
>>>>>>> master:AuthJanitor.Functions.AdminApi/Services/ManagedSecretsService.cs

            return new OkObjectResult(_managedSecretViewModel(newManagedSecret));
        }
    }
}

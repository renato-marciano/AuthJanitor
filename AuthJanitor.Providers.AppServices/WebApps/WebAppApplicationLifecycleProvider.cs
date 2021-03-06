﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using AuthJanitor.Extensions.Azure;
using Microsoft.Azure.Management.AppService.Fluent;
using System;
using System.Threading.Tasks;

namespace AuthJanitor.Providers.AppServices.WebApps
{
    public abstract class WebAppApplicationLifecycleProvider<TConsumerConfiguration> : SlottableApplicationLifecycleProvider<TConsumerConfiguration>
        where TConsumerConfiguration : SlottableProviderConfiguration
    {
        public override async Task Test()
        {
            var sourceDeploymentSlot = await GetDeploymentSlot(SourceSlotName);
            if (sourceDeploymentSlot == null) throw new Exception("Source Deployment Slot is invalid");

            var temporaryDeploymentSlot = await GetDeploymentSlot(TemporarySlotName);
            if (temporaryDeploymentSlot == null) throw new Exception("Temporary Deployment Slot is invalid");

            var destinationDeploymentSlot = await GetDeploymentSlot(DestinationSlotName);
            if (destinationDeploymentSlot == null) throw new Exception("Destination Deployment Slot is invalid");
        }

        protected async Task<IWebApp> GetWebApp()
        {
            return await (await this.GetAzure()).WebApps.GetByResourceGroupAsync(ResourceGroup, ResourceName);
        }

        protected async Task<IDeploymentSlot> GetDeploymentSlot(string name)
        {
            return await (await GetWebApp()).DeploymentSlots.GetByNameAsync(name);
        }
    }
}

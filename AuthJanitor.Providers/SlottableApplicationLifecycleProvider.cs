﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace AuthJanitor.Providers
{
    /// <summary>
    /// Describes a ConsumingApplication which implements a slot pattern (such as Functions or WebApps)
    /// </summary>
    public abstract class SlottableApplicationLifecycleProvider<TSlottableProviderConfiguration> :
        ApplicationLifecycleProvider<TSlottableProviderConfiguration>
        where TSlottableProviderConfiguration : SlottableProviderConfiguration
    {
        /// <summary>
        /// Source Slot (original application)
        /// </summary>
        public string SourceSlotName => Configuration.SourceSlot;

        /// <summary>
        /// Temporary Slot (to coalesce new keys/configuration)
        /// </summary>
        public string TemporarySlotName => Configuration.TemporarySlot;

        /// <summary>
        /// Destination Slot (updated application). By default this is the same as the Source Slot.
        /// </summary>
        public string DestinationSlotName => Configuration.DestinationSlot;
    }
}

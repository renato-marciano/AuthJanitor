﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using AuthJanitor.Integrations.DataStores;
using System;

namespace AuthJanitor.Automation.Shared.Models
{
    public class Resource : IAuthJanitorModel
    {
        public Guid ObjectId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsRekeyableObjectProvider { get; set; }
        public string ProviderType { get; set; }
        public string ProviderConfiguration { get; set; }
    }
}

﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Azure.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthJanitor.Extensions.Azure
{
    public class ExistingTokenCredential : TokenCredential
    {
        private readonly AccessToken _accessToken;
        public ExistingTokenCredential(string accessToken, DateTimeOffset expiresOn)
        {
            _accessToken = new AccessToken(accessToken, expiresOn);
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return _accessToken;
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new ValueTask<AccessToken>(_accessToken);
        }
    }
}

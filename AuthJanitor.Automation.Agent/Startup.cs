﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using AuthJanitor.Automation.Shared;
using AuthJanitor.Integrations.CryptographicImplementations.Default;
using AuthJanitor.Integrations.DataStores.AzureBlobStorage;
using AuthJanitor.Integrations.IdentityServices.AzureActiveDirectory;
using AuthJanitor.Integrations.SecureStorage.AzureKeyVault;
using AuthJanitor.Providers;
using McMaster.NETCore.Plugins;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: FunctionsStartup(typeof(AuthJanitor.Automation.Agent.Startup))]
namespace AuthJanitor.Automation.Agent
{
    public class Startup : FunctionsStartup
    {
        private const string PROVIDER_SEARCH_MASK = "AuthJanitor.Providers.*.dll";
        private static readonly string PROVIDER_SEARCH_PATH = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
        private static readonly Type[] PROVIDER_SHARED_TYPES = new Type[]
        {
            typeof(IAuthJanitorProvider),
            typeof(AuthJanitorProvider<>),
            typeof(IApplicationLifecycleProvider),
            typeof(ApplicationLifecycleProvider<>),
            typeof(IRekeyableObjectProvider),
            typeof(RekeyableObjectProvider<>),
            typeof(IServiceCollection),
            typeof(ILogger)
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var logger = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug)
                       .AddConsole();
            }).CreateLogger<Startup>();

            builder.Services.AddOptions();

            logger.LogDebug("Registering Azure AD Identity Service");
            builder.Services.AddAJAzureActiveDirectory<AzureADIdentityServiceConfiguration>(o =>
            {
                o.ClientId = "clientId";
                o.ClientSecret = "clientSecret";
                o.TenantId = "tenantId";
            });

            logger.LogDebug("Registering Event Sinks");

            // TODO: Register IEventSinks here, before the EventDispatcherService
            //       This is where we offload to Azure Sentinel, send emails, etc.
            //       The *entire system* offloads to the EventDispatcherService to generalize events.

            logger.LogDebug("Registering Cryptographic Implementation");
            builder.Services.AddAJDefaultCryptographicImplementation<DefaultCryptographicImplementationConfiguration>(o =>
            {
                o.MasterEncryptionKey = "weakkey";
            });

            logger.LogDebug("Registering Secure Storage Provider");
            builder.Services.AddAJAzureKeyVault<KeyVaultSecureStorageProviderConfiguration>(o =>
            {
                o.VaultName = "vault";
            });

            logger.LogDebug("Registering AuthJanitor MetaServices");
            AuthJanitorServiceRegistration.RegisterServices(builder.Services);

            // -----

            logger.LogDebug("Registering DataStores");
            builder.Services.AddAJAzureBlobStorage<AzureBlobStorageDataStoreConfiguration>(o =>
            {
                o.ConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
                o.Container = "authjanitor";
            });

            // -----

            logger.LogDebug("Registering ViewModel generators");
            ViewModelFactory.ConfigureServices(builder.Services);

            // -----

            logger.LogDebug("Scanning for Provider modules at {ProviderSearchPath}\\{ProviderSearchMask} recursively", PROVIDER_SEARCH_PATH, PROVIDER_SEARCH_MASK);

            var providerTypes = Directory.GetFiles(PROVIDER_SEARCH_PATH, PROVIDER_SEARCH_MASK, new EnumerationOptions() { RecurseSubdirectories = true })
                                         .SelectMany(libraryFile => PluginLoader.CreateFromAssemblyFile(libraryFile, PROVIDER_SHARED_TYPES)
                                                                            .LoadDefaultAssembly()
                                                                            .GetTypes()
                                                                            .Where(type => !type.IsAbstract && typeof(IAuthJanitorProvider).IsAssignableFrom(type)))
                                         .ToArray();

            logger.LogInformation("Found {ProviderCount} providers: {ProviderTypeNames}", providerTypes.Length, string.Join("  ", providerTypes.Select(t => t.Name)));
            logger.LogInformation("Registering Provider Manager Service");
            ProviderManagerService.ConfigureServices(builder.Services, providerTypes);
        }
    }
}

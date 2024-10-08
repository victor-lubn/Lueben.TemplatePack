﻿// Copyright (c) IEvangelist. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Lueben.Templates.Microservice.Data.CosmosDb.Options;
using Lueben.Templates.Microservice.Data.CosmosDb.Programmability;
using Lueben.Templates.Microservice.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Templates.Microservice.Data.CosmosDb.Providers
{
    [ExcludeFromCodeCoverage]
    public class DefaultCosmosContainerProvider<TItem>
        : ICosmosContainerProvider<TItem>
        where TItem : Entity
    {
        private readonly Lazy<Task<Container>> _lazyContainer;
        private readonly RepositoryOptions _options;
        private readonly ICosmosClientProvider _cosmosClientProvider;
        private readonly ICosmosPartitionKeyPathProvider _cosmosPartitionKeyPathProvider;
        private readonly ILogger<DefaultCosmosContainerProvider<TItem>> _logger;

        public DefaultCosmosContainerProvider(
            ICosmosClientProvider cosmosClientProvider,
            ICosmosPartitionKeyPathProvider cosmosPartitionKeyPathProvider,
            IOptions<RepositoryOptions> options,
            ILogger<DefaultCosmosContainerProvider<TItem>> logger,
            ICosmosProgrammabilityFactory programmabilityFactory)
        {
            _cosmosClientProvider = cosmosClientProvider
                ?? throw new ArgumentNullException(
                    nameof(cosmosClientProvider), "Cosmos client provider is required.");

            _cosmosPartitionKeyPathProvider = cosmosPartitionKeyPathProvider
                ?? throw new ArgumentNullException(
                    nameof(cosmosPartitionKeyPathProvider), "Cosmos partition key name provider is required.");

            _options = options?.Value
                ?? throw new ArgumentNullException(
                    nameof(options), "Repository options are required.");

            if (_options.CosmosConnectionString is null)
            {
                throw new ArgumentNullException($"The {nameof(_options.CosmosConnectionString)} is required.");
            }

            if (_options.ContainerPerItemType is false)
            {
                if (_options.DatabaseId is null)
                {
                    throw new ArgumentNullException(
                        $"The {nameof(_options.DatabaseId)} is required when container per item type is false.");
                }

                if (_options.ContainerId is null)
                {
                    throw new ArgumentNullException(
                        $"The {nameof(_options.ContainerId)} is required when container per item type is false.");
                }
            }

            if (logger is null)
            {
                throw new ArgumentNullException($"The {nameof(logger)} is required.");
            }

            _logger = logger;
            _lazyContainer = new Lazy<Task<Container>>(async () =>
            {
                try
                {
                    Database database =
                        await _cosmosClientProvider.UseClientAsync(
                            client => client.CreateDatabaseIfNotExistsAsync(_options.DatabaseId)).ConfigureAwait(false);

                    ContainerProperties containerProperties = new ContainerProperties
                    {
                        Id = _options.ContainerPerItemType ? typeof(TItem).Name : _options.ContainerId,
                        PartitionKeyPath = _cosmosPartitionKeyPathProvider.GetPartitionKeyPath<TItem>()
                    };

                    Container container =
                        await database.CreateContainerIfNotExistsAsync(
                            containerProperties).ConfigureAwait(false);

                    await programmabilityFactory.InitializeProgrammability(container);

                    return container;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);

                    throw;
                }
            });
        }

        public Task<Container> GetContainerAsync() => _lazyContainer.Value;
    }
}

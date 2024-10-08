﻿using Microsoft.Azure.Cosmos;

namespace Lueben.Templates.Microservice.Data.CosmosDb.Providers
{
    /// <summary>
    /// The cosmos client options provider exposes a means of providing
    /// an instance to the configured <see cref="CosmosClientOptions"/> object,
    /// which is shared.
    /// </summary>
    public interface ICosmosClientOptionsProvider
    {
        /// <summary>
        /// Gets the configured <see cref="CosmosClientOptions"/> instance.
        /// </summary>
        /// <returns>A <see cref="CosmosClientOptions"/> instance.</returns>
        CosmosClientOptions ClientOptions { get; }
    }
}

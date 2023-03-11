﻿namespace Genocs.Persistence.MongoDb
{
    using Genocs.Persistence.MongoDb.Options;
    using Microsoft.Extensions.Options;
    using MongoDB.Driver;
    using MongoDB.Driver.Core.Extensions.DiagnosticSources;
    using System;

    /// <summary>
    /// The MongoDatabaseProvider
    /// </summary>
    public class MongoDatabaseProvider : IMongoDatabaseProvider
    {
        /// <summary>
        /// Reference to MongoClient
        /// </summary>
        public IMongoClient MongoClient { get; private set; }

        /// <summary>
        /// Reference to Database
        /// </summary>
        public IMongoDatabase Database { get; private set; }


        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="options"></param>
        /// <exception cref="NullReferenceException"></exception>
        public MongoDatabaseProvider(IOptions<MongoDbSettings> options)
        {
            if (options == null) throw new NullReferenceException(nameof(options));
            MongoDbSettings dBSettings = options.Value;

            if (dBSettings == null) throw new NullReferenceException(nameof(dBSettings));

            if (!MongoDbSettings.IsValid(dBSettings)) throw new InvalidOperationException($"{nameof(dBSettings)} is invalid");

            MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(dBSettings.ConnectionString);

            if (dBSettings.EnableTracing)
            {
                clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            }

            this.MongoClient = new MongoClient(clientSettings);
            this.Database = this.MongoClient.GetDatabase(dBSettings.Database);
        }
    }
}

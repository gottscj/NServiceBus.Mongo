using System;
using NServiceBus.Features;
using NServiceBus.Mongo.Subscriptions;
using NServiceBus.Mongo.Timeouts;
using NServiceBus.Persistence;

namespace NServiceBus.Mongo
{
    public class MongoPersistenceDefinition : PersistenceDefinition
    {
        public MongoPersistenceDefinition()
        {
            Defaults(settings => { settings.EnableFeatureByDefault<MongoStorageFeature>(); });
            
            Supports<StorageType.Timeouts>(s => s.EnableFeatureByDefault<MongoTimeoutFeature>());
            Supports<StorageType.Subscriptions>(s => s.EnableFeatureByDefault<MongoSubscriptionFeature>());
        }
    }
}
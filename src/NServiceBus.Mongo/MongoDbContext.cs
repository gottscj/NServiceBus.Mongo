using MongoDB.Bson;
using MongoDB.Driver;
using NServiceBus.Mongo.Subscriptions;
using NServiceBus.Mongo.Timeouts;
using NServiceBus.Timeout.Core;
using static MongoDB.Driver.Builders<NServiceBus.Mongo.Timeouts.MongoTimeoutData>;

namespace NServiceBus.Mongo
{
    public interface IMongoDbContext
    {
        IMongoCollection<MongoTimeoutData> Timeouts { get; }
        
        IMongoCollection<MongoSubscriptionData> Subscriptions { get; }
        
        IMongoDatabase Database { get; }
    }
    public class MongoDbContext : IMongoDbContext
    {
        public IMongoDatabase Database { get; }

        public MongoDbContext(string connectionString)
        {
            var mongoUrl = MongoUrl.Create(connectionString);
            var settings = MongoClientSettings.FromUrl(mongoUrl);
            
            var client = new MongoClient(settings);
            var databaseName = mongoUrl.DatabaseName ?? "NServiceBusStorage";
            Database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<MongoTimeoutData> Timeouts =>
            Database.GetCollection<MongoTimeoutData>("timeouts");

        public IMongoCollection<MongoSubscriptionData> Subscriptions =>
            Database.GetCollection<MongoSubscriptionData>("subscriptions");
        
        
        internal void EnsureIndexes()
        {
            var sagaIndexModel =
                new CreateIndexModel<MongoTimeoutData>(IndexKeys.Ascending(t => t.SagaId));

            var endpointIndexModel =
                new CreateIndexModel<MongoTimeoutData>(IndexKeys.Ascending(t => t.Endpoint));


            var lockDateTimeIndexModel =
                new CreateIndexModel<MongoTimeoutData>(IndexKeys.Ascending(t => t.LockDateTime));

            Timeouts.Indexes.CreateOne(sagaIndexModel);
            Timeouts.Indexes.CreateOne(endpointIndexModel);
            Timeouts.Indexes.CreateOne(lockDateTimeIndexModel);
        }
    }
}
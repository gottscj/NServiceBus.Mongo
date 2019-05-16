using MongoDB.Driver;
using NServiceBus.Mongo.Gateway;
using NServiceBus.Mongo.Subscriptions;
using NServiceBus.Mongo.Timeouts;

namespace NServiceBus.Mongo
{
    public interface IMongoDbContext
    {
        IMongoCollection<MongoTimeoutData> Timeouts { get; }
        
        IMongoCollection<MongoSubscription> Subscriptions { get; }
        
        IMongoCollection<MongoGatewayMessage> GatewayMessages { get; }
        
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

        public IMongoCollection<MongoSubscription> Subscriptions =>
            Database.GetCollection<MongoSubscription>("subscriptions");

        public IMongoCollection<MongoGatewayMessage> GatewayMessages =>
            Database.GetCollection<MongoGatewayMessage>("gatewayMessages");
        
        
        internal void EnsureIndexes()
        {
            // ---- TIMEOUTS ----
            var timeoutIndexes = Builders<MongoTimeoutData>.IndexKeys;
            var sagaIndexModel =
                new CreateIndexModel<MongoTimeoutData>(timeoutIndexes.Ascending(t => t.SagaId));

            var endpointIndexModel =
                new CreateIndexModel<MongoTimeoutData>(timeoutIndexes.Ascending(t => t.Endpoint));


            var lockDateTimeIndexModel =
                new CreateIndexModel<MongoTimeoutData>(timeoutIndexes.Ascending(t => t.LockDateTime));

            Timeouts.Indexes.CreateMany(new[] {sagaIndexModel, endpointIndexModel, lockDateTimeIndexModel});
            
            // ---- SUBSCRIPTIONS ----
            var subscriptionIndexes = Builders<MongoSubscription>.IndexKeys;

            var messageTypeIndex =
                new CreateIndexModel<MongoSubscription>(subscriptionIndexes.Ascending(t => t.MessageTypeString));

            var transportAddressIndex =
                new CreateIndexModel<MongoSubscription>(subscriptionIndexes.Ascending(t => t.TransportAddress));

            var endpointIndex = new CreateIndexModel<MongoSubscription>(subscriptionIndexes.Ascending(t => t.Endpoint));

            Subscriptions.Indexes.CreateMany(new[] {messageTypeIndex, transportAddressIndex, endpointIndex});
        }
    }
}
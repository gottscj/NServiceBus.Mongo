using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Extensibility;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;
using MongoDB.Driver;

namespace NServiceBus.Mongo.Subscriptions
{
    public class MongoSubscriptions : IInitializableSubscriptionStorage
    {
        private readonly IMongoDbContext _mongoDbContext;

        public MongoSubscriptions(IMongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }
        public Task Subscribe(Subscriber subscriber, MessageType messageType, ContextBag context)
        {
            return _mongoDbContext
                .Subscriptions
                .InsertOneAsync(new MongoSubscriptionData
                {
                    MessageTypeString = messageType.ToString(),
                    Endpoint = subscriber.Endpoint,
                    TransportAddress = subscriber.TransportAddress
                });
        }

        public Task Unsubscribe(Subscriber subscriber, MessageType messageType, ContextBag context)
        {
            var messageTypeString = messageType.ToString();
            return _mongoDbContext
                .Subscriptions
                .DeleteManyAsync(s =>
                    s.MessageTypeString == messageTypeString &&
                    s.Endpoint == subscriber.Endpoint && s.TransportAddress == subscriber.TransportAddress);
        }

        public async Task<IEnumerable<Subscriber>> GetSubscriberAddressesForMessage(IEnumerable<MessageType> messageTypes, ContextBag context)
        {
            var messageTypeStrings = messageTypes.Select(m => m.ToString());
            
            var subscribers = await _mongoDbContext
                .Subscriptions
                .Find(s => messageTypeStrings.Contains(s.MessageTypeString))
                .Project(s => new {s.TransportAddress, s.Endpoint})
                .ToListAsync();

            return subscribers.Select(s => new Subscriber(s.TransportAddress, s.Endpoint));
        }

        public void Init()
        {
            throw new System.NotImplementedException();
        }
    }
}
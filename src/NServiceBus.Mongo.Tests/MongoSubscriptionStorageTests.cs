using System.Linq;
using AutoFixture;
using MongoDB.Bson;
using MongoDB.Driver;
using NServiceBus.Extensibility;
using NServiceBus.Mongo.Subscriptions;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;
using NUnit.Framework;
using Tests;

namespace NServiceBus.Mongo.Tests
{
    [TestFixture]
    public class MongoSubscriptionStorageTests
    {
        private MongoSubscriptionStorage _subscriptionStorage;
        private MongoDbContext _dbContext;
        private const string TestEndpoint = "TestEndpoint";
        private const string TestTransport = "TestTransport";
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _dbContext = new MongoDbContext(MongoConnectionUtils.GetConnectionString());
            _dbContext.EnsureIndexes();
            
            _subscriptionStorage = new MongoSubscriptionStorage(_dbContext);
        }
        [SetUp]
        public void Setup()
        {
            _dbContext.Database.DropCollection(_dbContext.Subscriptions.CollectionNamespace.CollectionName);
        }
        [Test]
        public void Subscribe_NewSubscription_Success()
        {
            // ARRANGE
            var subscriber = new Subscriber(TestTransport, TestEndpoint);
            var messageType = new MessageType(typeof(object));
            
            // ACT
            _subscriptionStorage.Subscribe(subscriber, messageType, new ContextBag()).GetAwaiter().GetResult();
            
            // ASSERT
            var mongoSubscription = _dbContext.Subscriptions.Find(new BsonDocument()).Single();
            
            Assert.That(mongoSubscription.MessageTypeString, Is.EqualTo(messageType.ToString()));
            Assert.That(mongoSubscription.Endpoint, Is.EqualTo(TestEndpoint));
            Assert.That(mongoSubscription.TransportAddress, Is.EqualTo(TestTransport));
        }
        
        [Test]
        public void Unsubscribe_SubscriptionExists_Success()
        {
            // ARRANGE
            var subscriber = new Subscriber(TestTransport, TestEndpoint);
            var messageType = new MessageType(typeof(object));
            _dbContext.Subscriptions.InsertOne(new MongoSubscription
            {
                Endpoint = subscriber.Endpoint,
                TransportAddress = subscriber.TransportAddress,
                Id = ObjectId.GenerateNewId(),
                MessageTypeString = messageType.ToString()
            });
            
            // ACT
            _subscriptionStorage.Unsubscribe(subscriber, messageType, new ContextBag()).GetAwaiter().GetResult();
            
            // ASSERT
            var mongoSubscriptions = _dbContext.Subscriptions.Find(new BsonDocument()).ToList();
            Assert.That(mongoSubscriptions, Is.Empty);
        }
        
        [Test]
        public void GetSubscriberAddressesForMessage_SubscriptionExists_DistinctListReturned()
        {
            // ARRANGE
            var subscriber = new Subscriber(TestTransport, TestEndpoint);
            var messageType1 = new MessageType(typeof(object));
            var messageType2 = new MessageType(typeof(string));
            
            _dbContext.Subscriptions.InsertOne(new MongoSubscription
            {
                Endpoint = subscriber.Endpoint,
                TransportAddress = subscriber.TransportAddress,
                Id = ObjectId.GenerateNewId(),
                MessageTypeString = messageType1.ToString()
            });
            _dbContext.Subscriptions.InsertOne(new MongoSubscription
            {
                Endpoint = subscriber.Endpoint,
                TransportAddress = subscriber.TransportAddress,
                Id = ObjectId.GenerateNewId(),
                MessageTypeString = messageType2.ToString()
            });
            
            // ACT
            var subscribers = _subscriptionStorage
                .GetSubscriberAddressesForMessage(new []{messageType1, messageType2}, new ContextBag())
                .GetAwaiter()
                .GetResult()
                .ToList();
            
            // ASSERT
            Assert.That(subscribers, Has.Count.EqualTo(1));
        }
    }
}
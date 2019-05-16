using System;
using AutoFixture;
using NServiceBus.Extensibility;
using NServiceBus.Mongo.Gateway;
using NUnit.Framework;
using Tests;

namespace NServiceBus.Mongo.Tests
{
    public class MongoDeduplicateMessagesTests
    {
        private MongoDeduplicateMessages _mongoDeduplicateMessages;
        private MongoDbContext _dbContext;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _dbContext = new MongoDbContext(MongoConnectionUtils.GetConnectionString());
            _dbContext.EnsureIndexes();
            
            _mongoDeduplicateMessages = new MongoDeduplicateMessages(_dbContext);
        }

        [SetUp]
        public void Setup()
        {
            _dbContext.Database.DropCollection(_dbContext.GatewayMessages.CollectionNamespace.CollectionName);
        }
        
        [Test]
        public void DeduplicateMessage_New_ReturnsTrue()
        {
            // ARRANGE
            
            // ACT
            var result = _mongoDeduplicateMessages
                .DeduplicateMessage("test", DateTime.Now, new ContextBag())
                .GetAwaiter()
                .GetResult();
            
            // ASSERT
            Assert.That(result, Is.True);
        }
        
        [Test]
        public void DeduplicateMessage_Exists_ReturnsFalse()
        {
            // ARRANGE
            var gatewayMessage = new MongoGatewayMessage {Id = "test", TimeReceived = DateTime.Now};
            _dbContext.GatewayMessages.InsertOne(gatewayMessage);
            
            // ACT
            var result = _mongoDeduplicateMessages
                .DeduplicateMessage("test", DateTime.Now, new ContextBag())
                .GetAwaiter()
                .GetResult();
            
            // ASSERT
            Assert.That(result, Is.False);
        }
    }
}
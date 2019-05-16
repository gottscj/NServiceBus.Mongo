using System;
using System.Collections.Generic;
using MongoDB.Driver;
using NServiceBus.Timeout.Core;
using NUnit.Framework;
using Tests;
using AutoFixture;
using NServiceBus.Extensibility;
using NServiceBus.Mongo.Timeouts;

namespace NServiceBus.Mongo.Tests
{
    [TestFixture]
    public class MongoTimeoutsTests
    {
        private MongoTimeouts _mongoTimeouts;
        private MongoDbContext _dbContext;
        private IFixture _fixture;
        private const string TestEndpoint = "MyTestEndpoint";
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _dbContext = new MongoDbContext(MongoConnectionUtils.GetConnectionString());
            _dbContext.Database.DropCollection(_dbContext.Timeouts.CollectionNamespace.CollectionName);
            _dbContext.EnsureIndexes();
            
            _mongoTimeouts = new MongoTimeouts(TestEndpoint, _dbContext);
            _fixture = new Fixture();
        }

        [Test]
        public void Add_NewTimeoutData_Added()
        {
            // ARRANGE
            var timeoutData = _fixture.Create<TimeoutData>();
            
            // ACT
            _mongoTimeouts.Add(timeoutData, new ContextBag()).GetAwaiter().GetResult();
            
            // ASSERT
            var timeoutFromDb = _dbContext.Timeouts.Find(t => t.Id == timeoutData.Id).SingleOrDefault();
            Assert.NotNull(timeoutFromDb);
        }
        
        [Test]
        public void GetNextChunk_TimeoutsPresent_RetrieveCompleteList()
        {
            // ARRANGE
            const int numberOfTimeoutsToAdd = 10;
            var timeoutData = new List<MongoTimeoutData>();

            for (var i = 0; i < numberOfTimeoutsToAdd; i++)
            {
                timeoutData.Add(MongoTimeoutData.Create(new TimeoutData
                {
                    Id = Guid.NewGuid().ToString(),
                    Time = DateTime.UtcNow.AddHours(-1),
                    Destination = $"timeouts@{Environment.MachineName}",
                    SagaId = Guid.NewGuid(),
                    State = new byte[] {0, 0, 133},
                    Headers = new Dictionary<string, string>{{"Bar", "34234"}, {"Foo", "aString1"}, {"Super", "aString2"}},
                    OwningTimeoutManager = TestEndpoint,
                }));
            }
            _dbContext.Timeouts.InsertMany(timeoutData);

            // ACT
            var result = _mongoTimeouts.GetNextChunk(DateTime.UtcNow.AddYears(-3)).GetAwaiter().GetResult();
            
            // ASSERT
            Assert.AreEqual(numberOfTimeoutsToAdd, result.DueTimeouts.Length);
        }
        
        [Test]
        public void GetNextChunk_TimeoutPresent_NextTimeToQueryOk()
        {
            // ARRANGE
            var nextTime = DateTime.UtcNow.AddHours(1);
            _dbContext.Timeouts.InsertOne(MongoTimeoutData.Create(new TimeoutData
            {
                Id = Guid.NewGuid().ToString(),
                Time = nextTime,
                Destination = $"timeouts@{Environment.MachineName}",
                SagaId = Guid.NewGuid(),
                State = new byte[] {0, 0, 133},
                Headers = new Dictionary<string, string> {{"Bar", "34234"}, {"Foo", "aString1"}, {"Super", "aString2"}},
                OwningTimeoutManager = TestEndpoint,
            }));

            // ACT
            var result = _mongoTimeouts.GetNextChunk(DateTime.UtcNow.AddYears(-3)).GetAwaiter().GetResult();

            // ASSERT
            Assert.IsTrue((nextTime - result.NextTimeToQuery).TotalSeconds < 1);
        }
        
        [Test]
        public void Peek_TimeoutPresent_FetchesTimeoutDataAndSetsLock()
        {
            // ARRANGE
            var now = DateTime.UtcNow;
            var timeoutData = MongoTimeoutData.Create(new TimeoutData
            {
                Id = Guid.NewGuid().ToString(),
                Time = now,
                Destination = $"timeouts@{Environment.MachineName}",
                SagaId = Guid.NewGuid(),
                State = new byte[] {0, 0, 133},
                Headers = new Dictionary<string, string> {{"Bar", "34234"}, {"Foo", "aString1"}, {"Super", "aString2"}},
                OwningTimeoutManager = TestEndpoint,
            });
            _dbContext.Timeouts.InsertOne(timeoutData);

            // ACT
            var result = (MongoTimeoutData)_mongoTimeouts.Peek(timeoutData.Id, new ContextBag()).GetAwaiter().GetResult();

            // ASSERT
            Assert.NotNull(result.LockDateTime);
            Assert.IsTrue((now - result.LockDateTime.Value).TotalSeconds < 1);
        }
    }
}
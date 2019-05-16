using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus.Extensibility;
using NServiceBus.Gateway.Deduplication;

namespace NServiceBus.Mongo.Gateway
{
    public class MongoDeduplicateMessages : IDeduplicateMessages
    {
        private readonly IMongoDbContext _dbContext;

        public MongoDeduplicateMessages(IMongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> DeduplicateMessage(string clientId, DateTime timeReceived, ContextBag context)
        {
            try
            {
                await _dbContext
                    .GatewayMessages
                    .WithWriteConcern(WriteConcern.W1)
                    .WithReadPreference(ReadPreference.Primary)
                    .InsertOneAsync(new MongoGatewayMessage
                    {
                        Id = clientId,
                        TimeReceived = timeReceived
                    });

                return true;
            }
            catch (MongoWriteException aggEx)
            {
                // Check for "E11000 duplicate key error"
                // https://docs.mongodb.org/manual/reference/command/insert/
                if (aggEx.WriteError?.Code == 11000)
                {
                    return false;
                }

                throw;
            }
        }
    }
}
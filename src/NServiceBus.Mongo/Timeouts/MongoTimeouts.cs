using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus.Extensibility;
using NServiceBus.Timeout.Core;

namespace NServiceBus.Mongo.Timeouts
{
    public class MongoTimeouts : IPersistTimeouts, IQueryTimeouts
    {
        private readonly string _endpoint;
        private readonly IMongoDbContext _dbContext;
        
        public MongoTimeouts(string endpoint, IMongoDbContext dbContext)
        {
            _endpoint = endpoint;
            _dbContext = dbContext;
        }
        
        public async Task Add(TimeoutData timeout, ContextBag context)
        {
            await _dbContext
                .Timeouts
                .InsertOneAsync(MongoTimeoutData.Create(timeout))
                .ConfigureAwait(false);
        }

        public async Task<bool> TryRemove(string timeoutId, ContextBag context)
        {
            var deleteResult = await _dbContext
                .Timeouts
                .DeleteOneAsync(t => t.Id == timeoutId)
                .ConfigureAwait(false);
            return deleteResult.DeletedCount != 0;
        }

        public async Task<TimeoutData> Peek(string timeoutId, ContextBag context)
        {
            var now = DateTime.UtcNow;
            var update = Builders<MongoTimeoutData>.Update.Set(t => t.LockDateTime, now);
            var options = new FindOneAndUpdateOptions<MongoTimeoutData>
            {
                ReturnDocument = ReturnDocument.After
            };

            var timeoutData = await _dbContext
                .Timeouts
                .FindOneAndUpdateAsync<MongoTimeoutData>(
                    t => t.Id == timeoutId && (!t.LockDateTime.HasValue || t.LockDateTime.Value < now.AddSeconds(-10)),
                    update, options)
                .ConfigureAwait(false);

            return timeoutData;
        }

        public Task RemoveTimeoutBy(Guid sagaId, ContextBag context)
        {
            return _dbContext
                .Timeouts.DeleteManyAsync(t => t.SagaId == sagaId);
        }

        public async Task<TimeoutsChunk> GetNextChunk(DateTime startSlice)
        {
            var now = DateTime.UtcNow;
            var sort = Builders<MongoTimeoutData>.Sort.Ascending(t => t.Time);
                
            var resultTask = _dbContext
                .Timeouts
                .Find(t => t.Endpoint == _endpoint && t.Time >= startSlice && t.Time < now)
                .Sort(sort)
                .Project(t => new { t.Id, t.Time })
                .ToListAsync();

            var startOfNextQueryTask = _dbContext
                .Timeouts
                .Find(t => t.Endpoint == _endpoint && t.Time >= now)
                .Sort(sort)
                .Project(t => new {t.Time})
                .FirstOrDefaultAsync();

            await Task.WhenAll(resultTask, startOfNextQueryTask).ConfigureAwait(false);

            var results = resultTask.Result;
            
            var nextTimeToRunQuery = startOfNextQueryTask.Result?.Time ?? DateTime.UtcNow.AddMinutes(10);
            
            return new TimeoutsChunk(
                results.Select(x => new TimeoutsChunk.Timeout(x.Id, x.Time)).ToArray(), 
                nextTimeToRunQuery);
        }
    }
}
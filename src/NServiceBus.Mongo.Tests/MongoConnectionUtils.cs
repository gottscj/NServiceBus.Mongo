using System.Collections.Generic;
using MongoDB.Driver;

namespace Tests
{
    public static class MongoConnectionUtils
    {
        public static string GetConnectionString()
        {
            return "mongodb://localhost:30000";
        }

        private static object _synRoot = new object();
        private static MongoClient _mongoClient = null;
        public static IMongoDatabase GetMongoDatabase()
        {
            lock (_synRoot)
            {
                if (_mongoClient == null)
                {
                    var settings = MongoClientSettings.FromConnectionString(GetConnectionString());

                    _mongoClient = new MongoClient(settings);
                }

                return _mongoClient.GetDatabase("NServiceBus-Tests");
            }
        }
    }
}
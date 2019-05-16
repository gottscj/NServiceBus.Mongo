using MongoDB.Driver;
using NServiceBus.Features;

namespace NServiceBus.Mongo
{
    public class MongoStorageFeature : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
            string connectionString = "NServiceBus/MongoDB";
            
            if (context.Settings.HasSetting("MongoDbConnectionString"))
            {
                connectionString = context.Settings.Get<string>("MongoDbConnectionString");
            }
            
            var mongoDbContext = new MongoDbContext(connectionString);
            mongoDbContext.EnsureIndexes();
            context.Container.RegisterSingleton((IMongoDbContext) mongoDbContext);
        }
    }
}
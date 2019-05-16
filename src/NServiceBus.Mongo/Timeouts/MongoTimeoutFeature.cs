using NServiceBus.Features;

namespace NServiceBus.Mongo.Timeouts
{
    public class MongoTimeoutFeature : Feature
    {
        internal MongoTimeoutFeature()
        {
            DependsOn<TimeoutManager>();
            DependsOn<MongoStorageFeature>();
        }
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent(
                b => new MongoTimeouts(context.Settings.EndpointName().ToString(), b.Build<IMongoDbContext>()),
                DependencyLifecycle.InstancePerCall);
        }
    }
}
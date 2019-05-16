using NServiceBus.Features;

namespace NServiceBus.Mongo.Gateway
{
    public class MongoGatewayFeature : Feature
    {
        public MongoGatewayFeature()
        {
            DependsOn("Gateway");
            DependsOn<MongoStorageFeature>();
        }
        protected override void Setup(FeatureConfigurationContext context)
        {
            
            context.Container.ConfigureComponent<MongoDeduplicateMessages>(DependencyLifecycle.InstancePerCall);
           
        }
    }
}
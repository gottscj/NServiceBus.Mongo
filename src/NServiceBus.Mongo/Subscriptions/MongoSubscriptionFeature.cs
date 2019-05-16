using NServiceBus.Features;

namespace NServiceBus.Mongo.Subscriptions
{
    public class MongoSubscriptionFeature : Feature
    {
        internal MongoSubscriptionFeature()
        {
            DependsOn<MessageDrivenSubscriptions>();
            DependsOn<MongoStorageFeature>();
        }
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<MongoSubscriptionStorage>(DependencyLifecycle.InstancePerCall);
        }
    }
}
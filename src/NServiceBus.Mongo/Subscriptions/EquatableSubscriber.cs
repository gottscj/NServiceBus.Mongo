using System;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;

namespace NServiceBus.Mongo.Subscriptions
{
    public class EquatableSubscriber : Subscriber, IEquatable<EquatableSubscriber>
    {
        public EquatableSubscriber(string transportAddress, string endpoint) 
            : base(transportAddress, endpoint)
        {
        }

        public bool Equals(EquatableSubscriber other)
        {
            return TransportAddress == other?.TransportAddress && Endpoint == other?.Endpoint;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EquatableSubscriber) obj);
        }

        public override int GetHashCode()
        {
            return (Endpoint + TransportAddress).GetHashCode();
        }
    }
}
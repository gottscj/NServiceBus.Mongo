using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NServiceBus.Mongo.Subscriptions
{
    public class MongoSubscriptionData
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string MessageTypeString { get; set; }

        public string TransportAddress { get; set; }

        public string Endpoint { get; set; }
    }
}
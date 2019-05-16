using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NServiceBus.Mongo.Gateway
{
    public class MongoGatewayMessage
    {
        [BsonId]
        public string Id { get; set; }

        public DateTime TimeReceived { get; set; }
    }
}
using System;
using NServiceBus.Timeout.Core;

namespace NServiceBus.Mongo.Timeouts
{
    public class MongoTimeoutData : TimeoutData
    {
        public static MongoTimeoutData Create(TimeoutData timeoutData)
        {
            return new MongoTimeoutData
            {
                Id = timeoutData.Id,
                SagaId = timeoutData.SagaId,
                Time = timeoutData.Time,
                State = timeoutData.State,
                Headers = timeoutData.Headers,
                Destination = timeoutData.Destination,
                OwningTimeoutManager = timeoutData.OwningTimeoutManager,
                Endpoint =  timeoutData.OwningTimeoutManager,
                LockDateTime = null
            };

        }
        
        /// <summary>
        /// Timeout endpoint name.
        /// </summary>
        public string Endpoint { get; set; }
        
        /// <summary>
        /// The time when the timeout record was locked. If null then the record has not been locked.
        /// </summary>
        /// <remarks>
        /// Timeout locks are only considered valid for 10 seconds, therefore if the LockDateTime is older than 10 seconds it is no longer valid.
        /// </remarks>
        public DateTime? LockDateTime { get; set; }
        
    }
}
using Confluent.Kafka;
using System;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// An implementation of <see cref="ISenderTransaction"/> for Kafka.
    /// </summary>
    public class KafkaSenderTransaction : ISenderTransaction
    {
        private readonly Func<SenderMessage, Message<Null, string>> _getKafkaMessageFunc;
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;

        internal KafkaSenderTransaction(Func<SenderMessage, Message<Null, string>> getKafkaMessageFunc, IProducer<Null, string> producer, string topic)
        {
            _getKafkaMessageFunc = getKafkaMessageFunc;
            _producer = producer;
            _topic = topic;
        }

        /// <summary>
        /// Adds the specified message to the transaction.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public void Add(SenderMessage message)
        {
            _producer.Produce(_topic, _getKafkaMessageFunc(message));
        }


        /// <summary>
        /// Commits any messages that were added to the transaction.
        /// </summary>
        public void Commit()
        {
            _producer.CommitTransaction(TimeSpan.FromSeconds(10));
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }


        /// <summary>
        /// Rolls back any messages that were added to the transaction.
        /// </summary>
        public void Rollback()
        {
            _producer.AbortTransaction(TimeSpan.FromSeconds(10));
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}

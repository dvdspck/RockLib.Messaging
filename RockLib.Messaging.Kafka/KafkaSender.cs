using Confluent.Kafka;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that sends messages to Kafka.
    /// </summary>
    public class KafkaSender : ITransactionalSender
    {
        private readonly Lazy<IProducer<Null, string>> _singletonProducer;
        private readonly ProducerBuilder<Null, string> _producerBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaSender"/> class.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="topic">The topic to produce messages to.</param>
        /// <param name="bootstrapServers">List of brokers as a CSV list of broker host or host:port.</param>
        /// <param name="messageTimeoutMs">
        /// Local message timeout. This value is only enforced locally and limits the time
        /// a produced message waits for successful delivery. A time of 0 is infinite. This
        /// is the maximum time librdkafka may use to deliver a message (including retries).
        /// Delivery error occurs when either the retry count or the message timeout are
        /// exceeded.
        /// </param>
        /// <param name="config">
        /// A collection of librdkafka configuration parameters (refer to
        /// https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md) and parameters
        /// specific to this client (refer to: Confluent.Kafka.ConfigPropertyNames).
        /// </param>
        public KafkaSender(string name, string topic, string bootstrapServers, int messageTimeoutMs = 10000, ProducerConfig config = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            Config = config ?? new ProducerConfig();
            Config.BootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers));
            Config.MessageTimeoutMs = Config.MessageTimeoutMs ?? messageTimeoutMs;

            _producerBuilder = new ProducerBuilder<Null, string>(Config);
            _producerBuilder.SetErrorHandler(OnError);

            _singletonProducer = new Lazy<IProducer<Null, string>>(() => _producerBuilder.Build());
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="KafkaSender"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the topic to subscribe to.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Gets the configuration that is used to create the <see cref="Producer{TKey, TValue}"/> for this receiver.
        /// </summary>
        public ProducerConfig Config { get; }

        /// <summary>
        /// Occurs when an error happens on a background thread.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            return _singletonProducer.Value.ProduceAsync(Topic, GetMessage(message));
        }

        /// <summary>
        /// Starts a message-sending transaction.
        /// </summary>
        /// <returns>
        /// An object representing the new transaction.
        /// </returns>
        public ISenderTransaction BeginTransaction()
        {
            var producer = _producerBuilder.Build();
            producer.InitTransactions(TimeSpan.FromSeconds(1));
            producer.BeginTransaction();

            return new KafkaSenderTransaction(GetMessage, producer, Topic);
        }

        /// <summary>
        /// Flushes the producer and disposes it.
        /// </summary>
        public void Dispose()
        {
            if (_singletonProducer.IsValueCreated)
            {
                _singletonProducer.Value.Flush(TimeSpan.FromSeconds(10));
                _singletonProducer.Value.Dispose();
            }
        }

        private static Message<Null, string> GetMessage(SenderMessage message)
        {
            if (message.OriginatingSystem == null)
                message.OriginatingSystem = "Kafka";

            var kafkaMessage = new Message<Null, string> { Value = message.StringPayload };

            if (message.Headers.Count > 0)
            {
                kafkaMessage.Headers = kafkaMessage.Headers ?? new Headers();
                foreach (var header in message.Headers)
                    kafkaMessage.Headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value.ToString()));
            }

            return kafkaMessage;
        }

        private void OnError(IProducer<Null, string> producer, Error error)
            => Error?.Invoke(this, new ErrorEventArgs(error.Reason, new KafkaException(error)));
    }
}

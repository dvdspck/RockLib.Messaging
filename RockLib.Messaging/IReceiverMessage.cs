using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RockLib.Messaging
{
    /// <summary>
    /// Defines the interface for a received message.
    /// </summary>
    public interface IReceiverMessage
    {
        /// <summary>
        /// Gets the priority of the received message.
        /// </summary>
        byte? Priority { get; }

        /// <summary>
        /// Gets the string value of the message. If the implemenation "speaks" binary,
        /// <paramref name="encoding"/> is used to convert the binary message to a string.
        /// If <paramref name="encoding"/> is null, the binary data will be converted using
        /// base 64 encoding.
        /// </summary>
        /// <param name="encoding">
        /// The encoding to use. A null value indicates that base 64 encoding should be used.
        /// </param>
        /// <returns>The string value of the message.</returns>
        string GetStringValue(Encoding encoding);

        /// <summary>
        /// Gets the binary value of the message. If the implemenation "speaks" string,
        /// <paramref name="encoding"/> is used to convert the string message to a byte array.
        /// If <paramref name="encoding"/> is null, the string data will be converted using
        /// base 64 encoding.
        /// </summary>
        /// <param name="encoding">
        /// The encoding to use. A null value indicates that base 64 encoding should be used.
        /// </param>
        /// <returns>The binary value of the message.</returns>
        byte[] GetBinaryValue(Encoding encoding);

        ReceiverMessageHeaders Headers { get; }

        /// <summary>
        /// Acknowledges the message, i
        /// </summary>
        void Acknowledge();

        /// <summary>
        /// Rolls the message back.
        /// </summary>
        void Rollback();

        bool IsTransactional { get; }

        /// <summary>
        /// Returns an instance of <see cref="ISenderMessage"/> that is equivalent to this
        /// instance of <see cref="IReceiverMessage"/>.
        /// </summary>
        ISenderMessage ToSenderMessage();
    }

    public sealed class ReceiverMessageHeaders : IReadOnlyDictionary<string, object>
    {
        private readonly IReadOnlyDictionary<string, object> _rawHeaders;

        public ReceiverMessageHeaders(IReadOnlyDictionary<string, object> rawHeaders)
        {
            _rawHeaders = rawHeaders ?? throw new ArgumentNullException(nameof(rawHeaders));
        }

        // get string header
        // get int header
        // get byte header

        public object this[string key] => _rawHeaders[key];

        public IEnumerable<string> Keys => _rawHeaders.Keys;

        public IEnumerable<object> Values => _rawHeaders.Values;

        public int Count => _rawHeaders.Count;

        public bool ContainsKey(string key)
        {
            return _rawHeaders.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _rawHeaders.GetEnumerator();
        }

        public bool TryGetValue(string key, out object value)
        {
            return _rawHeaders.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rawHeaders.GetEnumerator();
        }
    }
}
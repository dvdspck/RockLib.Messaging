using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RockLib.Messaging.CloudEvents
{
    public sealed class TraceParent
    {
        public const byte SampledFlag = 0x01;

        [ThreadStatic]
        private static readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        [ThreadStatic]
        private static readonly byte[] _traceIdBuffer = new byte[16];

        [ThreadStatic]
        private static readonly byte[] _parentIdBuffer = new byte[8];

        [ThreadStatic]
        private static readonly StringBuilder _stringBuilder = new StringBuilder(_traceIdBuffer.Length * 2);

        internal TraceParent()
        {
        }

        public void Parse(string traceParent)
        {
            const string pattern = "^([0-9a-f]{2})-([0-9a-f]{32})-([0-9a-f]{16})-([0-9a-f]{2})";
            var match = Regex.Match(traceParent, pattern);
            if (match.Success)
            {
                var version = match.Groups[1].Value;
                TraceId = match.Groups[2].Value;
                ParentId = match.Groups[3].Value;

                if (version != "ff"
                    && TraceId != "00000000000000000000000000000000"
                    && ParentId != "0000000000000000")
                {
                    var flags = byte.Parse(match.Groups[4].Value);
                    Sampled = (flags & SampledFlag) == SampledFlag;

                    SetValue();
                    return;
                }
            }

            RestartTrace();
        }

        public string Value { get; private set; }

        public string Version => "00";

        public string TraceId { get; private set; }

        public string ParentId { get; private set; }

        public bool Sampled { get; private set; }

        public void UpdateParent() =>
            NewParentId();

        public void UpdateParent(string parentId)
        {
            if (string.IsNullOrEmpty(parentId))
                throw new ArgumentNullException(nameof(parentId));

            if (parentId == "0000000000000000")
                throw NotAValidParentId(parentId);

            const string pattern = "^[0-9a-f]{16}$";
            if (Regex.IsMatch(parentId, pattern))
                ParentId = parentId;
            else
                throw NotAValidParentId(parentId);
        }

        public void UpdateSampled(bool sampled)
        {
            if (sampled == Sampled)
                return;

            NewParentId();
            Sampled = sampled;
        }

        public void RestartTrace()
        {
            NewTraceId();
            NewParentId();
            Sampled = false;
            SetValue();
        }

        private void SetValue() =>
            Value = $"{Version}-{TraceId}-{ParentId}-{(Sampled ? "01" : "00")}";

        private void NewTraceId() =>
            TraceId = GetRandomHexString(_traceIdBuffer);

        private void NewParentId() =>
            ParentId = GetRandomHexString(_parentIdBuffer);

        private static string GetRandomHexString(byte[] buffer)
        {
            _rng.GetBytes(buffer);
            _stringBuilder.Clear();
            foreach (byte b in buffer)
                _stringBuilder.AppendFormat("{0:x2}", b);
            return _stringBuilder.ToString();
        }

        private static Exception NotAValidParentId(string parentId) =>
            new FormatException($"'{parentId}' is not a valid parent-id.");
    }
}

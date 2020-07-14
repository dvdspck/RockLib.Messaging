namespace RockLib.Messaging.CloudEvents
{
    public class DistributedTracingEvent : CloudEvent
    {
        /// <summary>The name of the <see cref="TraceParent"/> attribute.</summary>
        public const string TraceParentAttribute = "traceparent";

        /// <summary>The name of the <see cref="TraceState"/> attribute.</summary>
        public const string TraceStateAttribute = "tracestate";

        public DistributedTracingEvent() : base() { }

        public DistributedTracingEvent(DistributedTracingEvent source)
            : base(source)
        {
            // TODO: Transfer traceparent and tracestate, making any necessary transformations according to the spec.
        }

        public DistributedTracingEvent(IReceiverMessage receiverMessage, IProtocolBinding protocolBinding)
            : base(receiverMessage, protocolBinding)
        {
            if (receiverMessage.Headers.TryGetValue(TraceParentAttribute, out string traceParent))
            {
                TraceParent.Parse(traceParent);

                if (receiverMessage.Headers.TryGetValue(TraceStateAttribute, out string traceState))
                {
                    TraceState.Parse(traceState);
                }
            }
        }

        public TraceParent TraceParent { get; } = new TraceParent();

        public TraceState TraceState { get; } = new TraceState();

        public override SenderMessage ToSenderMessage()
        {
            var senderMessage = base.ToSenderMessage();

            // TODO: Implement

            return senderMessage;
        }

        public override void Validate()
        {
            base.Validate();
            
            // TODO: Implement
        }

        public static void Validate(SenderMessage senderMessage, IProtocolBinding protocolBinding = null)
        {
            CloudEvent.Validate(senderMessage, protocolBinding);

            // TODO: Implement
        }
    }
}

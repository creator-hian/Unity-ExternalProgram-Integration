using Hian.ExternalProgram.Core.Communication.Settings;

namespace Hian.ExternalProgram.Core.Communication.Protocols.Udp
{
    public class UdpSettingsImpl : ProtocolSettingsBase, IUdpSettings
    {
        public bool Broadcast { get; set; } = false;
        public bool MulticastLoopback { get; set; } = false;
        public int TimeToLive { get; set; } = 1;
        public bool DontFragment { get; set; } = false;
        public bool EnableReliableDelivery { get; set; } = false;
        public int AckTimeoutMs { get; set; } = 1000;
        public int MaxRetransmissions { get; set; } = 3;
        public int MaxPacketSize { get; set; } = 1400;
        public bool EnableMulticast { get; set; } = false;
        public string MulticastGroup { get; set; } = "239.255.255.250";
        public int MulticastPort { get; set; } = 1900;

        public override object Clone()
        {
            return new UdpSettingsImpl
            {
                BufferSize = BufferSize,
                MaxMessageSize = MaxMessageSize,
                ConnectionTimeoutMs = ConnectionTimeoutMs,
                SendTimeoutMs = SendTimeoutMs,
                ReceiveTimeoutMs = ReceiveTimeoutMs,
                EnableReconnect = EnableReconnect,
                MaxReconnectAttempts = MaxReconnectAttempts,
                ReconnectDelayMs = ReconnectDelayMs,
                MaxReconnectDelayMs = MaxReconnectDelayMs,
                EnableCompression = EnableCompression,
                EnableEncryption = EnableEncryption,
                MaxConcurrentOperations = MaxConcurrentOperations,
                EnableDebugLogging = EnableDebugLogging,
                EnableMetrics = EnableMetrics,
                MaxMetricsCount = MaxMetricsCount,
                Broadcast = Broadcast,
                MulticastLoopback = MulticastLoopback,
                TimeToLive = TimeToLive,
                DontFragment = DontFragment,
                EnableReliableDelivery = EnableReliableDelivery,
                AckTimeoutMs = AckTimeoutMs,
                MaxRetransmissions = MaxRetransmissions,
                MaxPacketSize = MaxPacketSize,
                EnableMulticast = EnableMulticast,
                MulticastGroup = MulticastGroup,
                MulticastPort = MulticastPort,
            };
        }

        public override bool Validate()
        {
            if (!base.Validate())
                return false;
            if (TimeToLive <= 0)
                return false;
            if (AckTimeoutMs <= 0)
                return false;
            if (MaxRetransmissions <= 0)
                return false;
            if (MaxPacketSize <= 0)
                return false;
            if (MulticastPort <= 0 || MulticastPort > 65535)
                return false;
            return true;
        }
    }
}

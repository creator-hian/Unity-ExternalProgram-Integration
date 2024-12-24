using Hian.ExternalProgram.Core.Communication.Settings;

namespace Hian.ExternalProgram.Core.Communication.Protocols.Tcp
{
    public class TcpSettingsImpl : ProtocolSettingsBase, ITcpSettings
    {
        public bool NoDelay { get; set; } = true;
        public int LingerSeconds { get; set; } = 0;
        public bool ReuseAddress { get; set; } = false;
        public bool KeepAlive { get; set; } = true;
        public int KeepAliveInterval { get; set; } = 1000;
        public int KeepAliveTime { get; set; } = 5000;
        public int KeepAliveRetryCount { get; set; } = 3;
        public int SendBufferSize { get; set; } = 8192;
        public int ReceiveBufferSize { get; set; } = 8192;

        public override object Clone()
        {
            return new TcpSettingsImpl
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
                NoDelay = NoDelay,
                LingerSeconds = LingerSeconds,
                ReuseAddress = ReuseAddress,
                KeepAlive = KeepAlive,
                KeepAliveInterval = KeepAliveInterval,
                KeepAliveTime = KeepAliveTime,
                KeepAliveRetryCount = KeepAliveRetryCount,
                SendBufferSize = SendBufferSize,
                ReceiveBufferSize = ReceiveBufferSize,
            };
        }

        public override bool Validate()
        {
            if (!base.Validate())
            {
                return false;
            }

            if (LingerSeconds < 0)
            {
                return false;
            }

            if (KeepAliveInterval <= 0)
            {
                return false;
            }

            if (KeepAliveTime <= 0)
            {
                return false;
            }

            if (KeepAliveRetryCount <= 0)
            {
                return false;
            }

            if (SendBufferSize <= 0)
            {
                return false;
            }

            return ReceiveBufferSize > 0;
        }
    }
}

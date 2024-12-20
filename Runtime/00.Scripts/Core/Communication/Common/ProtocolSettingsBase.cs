namespace Hian.ExternalProgram.Core.Communication.Settings
{
    /// <summary>
    /// 프로토콜 설정의 기본 구현을 제공하는 추상 클래스입니다.
    /// </summary>
    public abstract class ProtocolSettingsBase : IProtocolSettings
    {
        public int BufferSize { get; set; } = 4096;
        public int MaxMessageSize { get; set; } = 65536;
        public int ConnectionTimeoutMs { get; set; } = 5000;
        public int SendTimeoutMs { get; set; } = 5000;
        public int ReceiveTimeoutMs { get; set; } = 5000;
        public bool EnableReconnect { get; set; } = true;
        public int MaxReconnectAttempts { get; set; } = 5;
        public int ReconnectDelayMs { get; set; } = 1000;
        public int MaxReconnectDelayMs { get; set; } = 30000;
        public bool EnableCompression { get; set; } = false;
        public bool EnableEncryption { get; set; } = false;
        public int MaxConcurrentOperations { get; set; } = 1;
        public bool EnableDebugLogging { get; set; } = false;
        public bool EnableMetrics { get; set; } = true;
        public int MaxMetricsCount { get; set; } = 1000;

        public virtual bool Validate()
        {
            if (BufferSize <= 0)
                return false;
            if (MaxMessageSize <= 0)
                return false;
            if (ConnectionTimeoutMs <= 0)
                return false;
            if (SendTimeoutMs <= 0)
                return false;
            if (ReceiveTimeoutMs <= 0)
                return false;
            if (MaxReconnectAttempts < 0)
                return false;
            if (ReconnectDelayMs < 0)
                return false;
            if (MaxReconnectDelayMs < 0)
                return false;
            if (MaxConcurrentOperations <= 0)
                return false;
            if (MaxMetricsCount < 0)
                return false;
            return true;
        }

        public abstract object Clone();
    }
}

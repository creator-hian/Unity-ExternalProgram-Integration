using System;

namespace Hian.ExternalProgram.Core.Communication.Settings
{
    /// <summary>
    /// 모든 프로토콜에서 공통으로 사용하는 설정 인터페이스
    /// </summary>
    public interface IProtocolSettings : ICloneable
    {
        // 버퍼 설정
        int BufferSize { get; set; }
        int MaxMessageSize { get; set; }

        // 타임아웃 설정
        int ConnectionTimeoutMs { get; set; }
        int SendTimeoutMs { get; set; }
        int ReceiveTimeoutMs { get; set; }

        // 재연결 설정
        bool EnableReconnect { get; set; }
        int MaxReconnectAttempts { get; set; }
        int ReconnectDelayMs { get; set; }
        int MaxReconnectDelayMs { get; set; }

        // 데이터 처리 설정
        bool EnableCompression { get; set; }
        bool EnableEncryption { get; set; }
        int MaxConcurrentOperations { get; set; }

        // 로깅 및 모니터링 설정
        bool EnableDebugLogging { get; set; }
        bool EnableMetrics { get; set; }
        int MaxMetricsCount { get; set; }

        // 유효성 검사
        bool Validate();
    }
}

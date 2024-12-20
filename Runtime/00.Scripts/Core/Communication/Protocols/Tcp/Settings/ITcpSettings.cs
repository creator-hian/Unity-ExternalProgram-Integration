using Hian.ExternalProgram.Core.Communication.Settings;

namespace Hian.ExternalProgram.Core.Communication.Protocols.Tcp
{
    /// <summary>
    /// TCP 프신 프로토콜의 설정을 정의하는 인터페이스입니다.
    /// </summary>
    public interface ITcpSettings : IProtocolSettings
    {
        // TCP 특화 설정
        /// <summary>
        /// Nagle 알고리즘의 비활성화 여부를 가져옵니다.
        /// </summary>
        bool NoDelay { get; }

        /// <summary>
        /// 소켓 연결 종료 시 대기 시간(초)을 가져옵니다.
        /// </summary>
        int LingerSeconds { get; }

        /// <summary>
        /// 주소 재사용 여부를 가져옵니다.
        /// </summary>
        bool ReuseAddress { get; }

        // Keep-Alive 설정
        /// <summary>
        /// Keep-Alive 기능의 활성화 여부를 가져옵니다.
        /// </summary>
        bool KeepAlive { get; }

        /// <summary>
        /// Keep-Alive 간격(밀리초)을 가져옵니다.
        /// </summary>
        int KeepAliveInterval { get; }

        /// <summary>
        /// Keep-Alive 시간(밀리초)을 가져옵니다.
        /// </summary>
        int KeepAliveTime { get; }

        /// <summary>
        /// Keep-Alive 재시도 횟수를 가져옵니다.
        /// </summary>
        int KeepAliveRetryCount { get; }

        // 소켓 버퍼 설정
        /// <summary>
        /// 송신 버퍼 크기를 가져옵니다.
        /// </summary>
        int SendBufferSize { get; }

        /// <summary>
        /// 수신 버퍼 크기를 가져옵니다.
        /// </summary>
        int ReceiveBufferSize { get; }
    }
}

using Hian.ExternalProgram.Core.Communication.Settings;

namespace Hian.ExternalProgram.Core.Communication.Protocols.Udp
{
    /// <summary>
    /// UDP 프신 프로토콜의 설정을 정의하는 인터페이스입니다.
    /// </summary>
    public interface IUdpSettings : IProtocolSettings
    {
        // UDP 특화 설정
        /// <summary>
        /// 브로드캐스트 활성화 여부를 가져오니다.
        /// </summary>
        bool Broadcast { get; }

        /// <summary>
        /// 멀티캐스트 루프백 활성화 여부를 가져오니다.
        /// </summary>
        bool MulticastLoopback { get; }

        /// <summary>
        /// TTL(Time To Live) 값을 가져오니다.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        /// 패킷 분할 방지 여부를 가져오니다.
        /// </summary>
        bool DontFragment { get; }

        // 신뢰성 관련 설정
        /// <summary>
        /// 신뢰성 있는 전송 기능의 활성화 여부를 가져오니다.
        /// </summary>
        bool EnableReliableDelivery { get; }

        /// <summary>
        /// ACK 타임아웃 시간(밀리초)을 가져오니다.
        /// </summary>
        int AckTimeoutMs { get; }

        /// <summary>
        /// 최대 재전송 횟수를 가져오니다.
        /// </summary>
        int MaxRetransmissions { get; }

        /// <summary>
        /// 최대 패킷 크기를 가져오니다.
        /// </summary>
        int MaxPacketSize { get; }

        // 멀티캐스트 설정
        /// <summary>
        /// 멀티캐스트 기능의 활성화 여부를 가져오니다.
        /// </summary>
        bool EnableMulticast { get; }

        /// <summary>
        /// 멀티캐스트 그룹 주소를 가져옵니다.
        /// </summary>
        string MulticastGroup { get; }

        /// <summary>
        /// 멀티캐스트 포트를 가져옵니다.
        /// </summary>
        int MulticastPort { get; }
    }
}

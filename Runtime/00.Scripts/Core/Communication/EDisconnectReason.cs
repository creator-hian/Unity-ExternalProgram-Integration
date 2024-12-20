namespace Hian.ExternalProgram.Core.Communication
{
    public enum EDisconnectReason
    {
        /// <summary>알 수 없는 이유</summary>
        Unknown,

        /// <summary>정상적인 종료</summary>
        Normal,

        /// <summary>타임아웃으로 인한 종료</summary>
        Timeout,

        /// <summary>네트워크 오류로 인한 종료</summary>
        NetworkError,

        /// <summary>프로토콜 오류로 인한 종료</summary>
        ProtocolError,

        /// <summary>원격 종단이 종료한 경우</summary>
        PeerGone,
    }
}

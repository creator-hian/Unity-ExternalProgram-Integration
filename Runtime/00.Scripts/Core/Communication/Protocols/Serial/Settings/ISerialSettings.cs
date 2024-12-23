using System.IO.Ports;
using Hian.ExternalProgram.Core.Communication.Settings;

namespace Hian.ExternalProgram.Core.Communication.Protocols.Serial
{
    /// <summary>
    /// 시리얼 통신 프로토콜의 설정을 정의하는 인터페이스입니다.
    /// </summary>
    public interface ISerialSettings : IProtocolSettings
    {
        // 기본 시리얼 설정
        /// <summary>
        /// 시리얼 포트의 전송 속도(baud rate)를 가져오니다.
        /// </summary>
        int BaudRate { get; }

        /// <summary>
        /// 데이터 비트 수를 가져옵니다.
        /// </summary>
        int DataBits { get; }

        /// <summary>
        /// 정지 비트를 가져옵니다.
        /// </summary>
        StopBits StopBits { get; }

        /// <summary>
        /// 패리티 검사 방식을 가져옵니다.
        /// </summary>
        Parity Parity { get; }

        /// <summary>
        /// 흐름 제어 방식을 가져옵니다.
        /// </summary>
        Handshake Handshake { get; }

        // 흐름 제어 설정
        /// <summary>
        /// DTR(Data Terminal Ready) 신호의 활성화 여부를 가져옵니다.
        /// </summary>
        bool DtrEnable { get; }

        /// <summary>
        /// RTS(Request To Send) 신호의 활성화 여부를 가져옵니다.
        /// </summary>
        bool RtsEnable { get; }

        /// <summary>
        /// Null 문자 무시 여부를 가져옵니다.
        /// </summary>
        bool DiscardNull { get; }

        /// <summary>
        /// 패리티 오류 발생 시 대체할 문자를 가져옵니다.
        /// </summary>
        byte ParityReplace { get; }

        // 고급 설정
        /// <summary>
        /// Break 신호 상태를 가져옵니다.
        /// </summary>
        bool BreakState { get; }

        /// <summary>
        /// Xoff 상태에서 전송 계속 여부를 가져옵니다.
        /// </summary>
        bool TxContinueOnXoff { get; }

        /// <summary>
        /// Xoff 문자를 가져옵니다.
        /// </summary>
        byte XoffChar { get; }

        /// <summary>
        /// Xon 문자를 가져옵니다.
        /// </summary>
        byte XonChar { get; }
    }
}

namespace FAMOZ.ExternalProgram.Core.Communication
{
    /// <summary>
    /// 통신 프로토콜 제공자 인터페이스입니다.
    /// 새로운 통신 프로토콜을 추가하려면 이 인터페이스를 구현하세요.
    /// </summary>
    public interface ICommunicationProtocolProvider
    {
        /// <summary>
        /// 프로토콜 타입을 식별하는 고유 문자열입니다.
        /// </summary>
        string ProtocolType { get; }

        /// <summary>
        /// 주어진 설정을 사용하여 새로운 통신 프로토콜 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="config">프로그램 설정</param>
        /// <returns>생성된 통신 프로토콜 인스턴스</returns>
        ICommunicationProtocol CreateProtocol(ProgramConfig config);
    }
} 
using System;
using System.Data;
using System.Threading.Tasks;

namespace Hian.ExternalProgram.Core.Communication
{
    public interface ICommunicationProtocol : IDisposable
    {
        /// <summary>
        /// 현재 TCP 연결의 상태를 확인합니다.
        /// </summary>
        bool IsConnected { get; }

        // 동기 메서드

        /// <summary>
        /// TCP 서버에 동기적으로 연결을 시도합니다.
        /// </summary>
        /// <returns>연결 성공 여부</returns>
        bool Connect();

        /// <summary>
        /// TCP 연결을 종료합니다.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 데이터를 동기적으로 전송합니다.
        /// </summary>
        /// <param name="data">전송할 데이터 바이트 배열</param>
        /// <returns>전송 성공 여부</returns>
        bool Send(byte[] data);

        /// <summary>
        /// 데이터를 동기적으로 수신합니다.
        /// </summary>
        /// <returns>수신된 데이터 바이트 배열 또는 null</returns>
        byte[] Receive();

        // 비동기 메서드

        /// <summary>
        /// TCP 서버에 비동기적으로 연결을 시도합니다.
        /// </summary>
        /// <returns>연결 성공 여부를 나타내는 Task</returns>
        Task<bool> ConnectAsync();

        /// <summary>
        /// TCP 연결을 비동기적으로 종료합니다.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// 데이터를 비동기적으로 전송합니다.
        /// </summary>
        /// <param name="data">전송할 데이터 바이트 배열</param>
        /// <returns>전송 성공 여부를 나타내는 Task</returns>
        Task<bool> SendAsync(byte[] data);

        /// <summary>
        /// 데이터를 비동기적으로 수신합니다.
        /// </summary>
        /// <returns>수신된 데이터 바이트 배열을 포함하는 Task 또는 null</returns>
        Task<byte[]> ReceiveAsync();

        // 이벤트
        /// <summary>데이터 수신 시 발생하는 이벤트</summary>
        event Action<byte[]> OnDataReceived;

        /// <summary>오류 발생 시 발생하는 이벤트</summary>
        event Action<Exception> OnError;

        /// <summary>
        /// 연결이 끊어졌을 때 발생하는 이벤트입니다.
        /// </summary>
        event Action OnDisconnected;

        /// <summary>
        /// 연결이 끊어진 이유와 함께 발생하는 이벤트입니다.
        /// </summary>
        event Action<EDisconnectReason> OnConnectionLost;

        /// <summary>
        /// 연결 상태가 변경되었을 때 발생하는 이벤트입니다.
        /// </summary>
        event Action<ConnectionState> OnStateChanged;
    }

    /// <summary>
    /// 연결 지향적 통신 프로토콜을 위한 인터페이스입니다.
    /// TCP나 Serial과 같은 연결 지향적 프로토콜에서 사용됩니다.
    /// </summary>
    public interface IConnectionOrientedProtocol : ICommunicationProtocol
    {
        /// <summary>
        /// 연결 시도 횟수가 변경될 때 발생하는 이벤트입니다.
        /// </summary>
        event Action<int> OnConnectionAttempt;

        /// <summary>
        /// 재연결 시도 시 대기 시간이 변경될 때 발생하는 이벤트입니다.
        /// </summary>
        event Action<TimeSpan> OnReconnecting;
    }
}

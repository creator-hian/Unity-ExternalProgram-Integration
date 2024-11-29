using System;
using System.Threading.Tasks;
using FAMOZ.ExternalProgram.Core.Communication;

namespace FAMOZ.ExternalProgram.Core
{
    /// <summary>
    /// 외부 프로그램과의 통신을 위한 인터페이스입니다.
    /// 기본 프로세스 관리 기능을 확장하여 통신 기능을 추가합니다.
    /// </summary>
    public interface IExternalProgram : IProcessManager
    {
        #region Constants
        /// <summary>프로세스가 시작되지 않은 상태를 나타내는 종료 코드</summary>
        const int PROCESS_NOT_STARTED = -1;
        /// <summary>프로세스가 실행 중임을 나타내는 종료 코드</summary>
        const int PROCESS_RUNNING = -2;
        #endregion

        #region Communication Properties
        /// <summary>통신 프로토콜</summary>
        ICommunicationProtocol CommunicationProtocol { get; }
        
        /// <summary>통신 연결 상태</summary>
        bool IsConnected { get; }
        #endregion

        #region Communication Control
        /// <summary>통신을 비동기적으로 연결합니다.</summary>
        Task<bool> ConnectAsync();
        
        /// <summary>통신을 비동기적으로 종료합니다.</summary>
        Task DisconnectAsync();
        
        /// <summary>통신을 동기적으로 연결합니다.</summary>
        bool Connect();
        
        /// <summary>통신을 동기적으로 종료합니다.</summary>
        void Disconnect();
        #endregion

        #region Communication Events
        /// <summary>프로그램 상태 변경 이벤트</summary>
        event Action<ProgramState> OnStateChanged;
        
        /// <summary>프로그램 에러 발생 이벤트</summary>
        event Action<ProgramError> OnError;
        #endregion

        #region Commands
        /// <summary>명령어를 비동기적으로 전송합니다.</summary>
        Task SendCommandAsync(string command);
        
        /// <summary>명령어를 비동기적으로 기다립니다.</summary>
        Task<string> WaitForResponseAsync(string expectedPattern, TimeSpan? timeout = null);
        #endregion
    }
} 

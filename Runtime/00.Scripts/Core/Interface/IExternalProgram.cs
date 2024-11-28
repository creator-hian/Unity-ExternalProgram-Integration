using System;
using System.Net;
using System.Threading.Tasks;

namespace FAMOZ.ExternalProgram.Core
{
    /// <summary>
    /// 외부 프로그램과의 통신을 위한 인터페이스
    /// </summary>
    public interface IExternalProgram : IDisposable
    {
        #region Properties
        /// <summary>
        /// 프로그램의 현재 상태
        /// </summary>
        ProgramState State { get; }
        
        /// <summary>
        /// 프로그램 설정
        /// </summary>
        ProgramConfig Config { get; }
        
        /// <summary>
        /// 프로세스 종료 코드 (null if not exited)
        /// </summary>
        int? ExitCode { get; }
        
        /// <summary>
        /// 프로그램과의 연결 상태
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// 프로그램의 IP 엔드포인트
        /// </summary>
        IPEndPoint EndPoint { get; }
        #endregion

        #region Events
        /// <summary>
        /// 프로그램 상태 변경 이벤트
        /// </summary>
        event Action<ProgramState> OnStateChanged;
        
        /// <summary>
        /// 프로그램으로부터 출력 수신 이벤트
        /// </summary>
        event Action<string> OnOutputReceived;
        
        /// <summary>
        /// 에러 발생 이벤트
        /// </summary>
        event Action<ProgramError> OnError;
        #endregion

        #region Methods
        /// <summary>
        /// 프로그램 시작
        /// </summary>
        Task StartAsync();
        
        /// <summary>
        /// 프로그램 정지
        /// </summary>
        Task StopAsync();
        
        /// <summary>
        /// 프로그램 재시작
        /// </summary>
        Task RestartAsync();
        
        /// <summary>
        /// 프로그램 실행 상태 확인
        /// </summary>
        Task<bool> IsRunningAsync();
        
        /// <summary>
        /// 명령어 전송
        /// </summary>
        /// <param name="command">전송할 명령어</param>
        Task SendCommandAsync(string command);
        
        /// <summary>
        /// 응답 대기
        /// </summary>
        /// <param name="expectedPattern">기대하는 응답 패턴</param>
        /// <param name="timeout">타임아웃 시간</param>
        Task<string> WaitForResponseAsync(
            string expectedPattern, 
            TimeSpan? timeout = null
        );
        
        /// <summary>
        /// 리소스 정리
        /// </summary>
        Task CleanupAsync();
        #endregion
    }
} 

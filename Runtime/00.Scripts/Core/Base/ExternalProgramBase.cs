using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace FAMOZ.ExternalProgram.Core
{
    /// <summary>
    /// 외부 프로그램 통신을 위한 기본 추상 클래스입니다.
    /// </summary>
    public abstract class ExternalProgramBase : IExternalProgram
    {
        #region Fields
        protected Process _process;
        protected readonly ProgramConfig _config;
        protected ProgramState _currentState = ProgramState.NotStarted;
        protected DateTime? _startTime;
        protected DateTime? _exitTime;
        protected int _failedAttempts;
        protected DateTime _lastResponseTime;
        #endregion

        #region Properties
        public ProgramState State => _currentState;
        public ProgramConfig Config => _config;

        public int ExitCode 
        {
            get 
            {
                if (_process == null)
                {
                    return IExternalProgram.PROCESS_NOT_STARTED;
                }
                
                if (!_process.HasExited)
                {
                    return IExternalProgram.PROCESS_RUNNING;
                }
                
                return _process.ExitCode;
            }
        }
        
        public abstract bool IsConnected { get; }
        
        public IPEndPoint EndPoint 
        {
            get 
            {
                var address = IPAddress.Parse(ExternalProgramConstants.LOCAL_ADDRESS);
                return new IPEndPoint(address, Config.PortNumber);
            }
        }
        
        public bool HasExited 
        {
            get 
            {
                if (_process == null)
                {
                    return true;
                }
                return _process.HasExited;
            }
        }
        
        public DateTime? StartTime => _startTime;
        public DateTime? ExitTime => _exitTime;
        public TimeSpan LastResponseTime => DateTime.Now - _lastResponseTime;
        public int FailedAttempts => _failedAttempts;
        public bool HasStarted => _startTime.HasValue;
        public bool HasCompleted => _exitTime.HasValue;
        
        public TimeSpan RunningTime 
        {
            get 
            {
                if (!_startTime.HasValue)
                {
                    return TimeSpan.Zero;
                }
                
                if (_exitTime.HasValue)
                {
                    return _exitTime.Value - _startTime.Value;
                }
                
                return DateTime.Now - _startTime.Value;
            }
        }
        #endregion

        #region Events
        public event Action<ProgramState> OnStateChanged;
        public event Action<string> OnOutputReceived;
        public event Action<ProgramError> OnError;
        #endregion

        protected ExternalProgramBase(ProgramConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        #region Protected Methods
        /// <summary>
        /// 프로그램 상태를 업데이트하고 이벤트를 발생시킵니다.
        /// </summary>
        protected virtual void UpdateState(ProgramState newState)
        {
            if (_currentState == newState) 
            {
                return;
            }
            
            _currentState = newState;
            if (OnStateChanged != null)
            {
                OnStateChanged(newState);
            }
            
            if (newState == ProgramState.Running)
            {
                _startTime = DateTime.Now;
                _exitTime = null;  // 재시작 시 종료 시간 초기화
            }
            else if (newState == ProgramState.Stopped)
            {
                _exitTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 에러를 발생시키고 로깅합니다.
        /// </summary>
        protected virtual void RaiseError(string message, ErrorType type, Exception ex = null)
        {
            var error = new ProgramError(message, type, ex);
            if (OnError != null)
            {
                OnError(error);
            }
            UnityEngine.Debug.LogError($"ExternalProgram Error: {error.Message}");
        }

        /// <summary>
        /// 프로세스 출력을 처리하고 이벤트를 발생시킵니다.
        /// </summary>
        protected virtual void HandleProcessOutput(string output)
        {
            if (string.IsNullOrEmpty(output)) 
            {
                return;
            }
            
            if (OnOutputReceived != null)
            {
                OnOutputReceived(output);
            }
            _lastResponseTime = DateTime.Now;
        }
        #endregion

        #region IExternalProgram Implementation
        public abstract Task StartAsync();
        public abstract Task StopAsync();
        public abstract Task<bool> IsRunningAsync();
        public abstract Task SendCommandAsync(string command);
        public abstract Task<string> WaitForResponseAsync(string expectedPattern, TimeSpan? timeout = null);
        
        public virtual async Task RestartAsync()
        {
            await StopAsync();
            await StartAsync();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task UpdateConfigAsync(ProgramConfig newConfig)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (State == ProgramState.Running)
                throw new InvalidOperationException("Cannot update config while program is running");
                
            // 설정 업데이트 로직
        }

        public virtual async Task CleanupAsync()
        {
            try
            {
                if (_process == null)
                {
                    return;
                }

                if (!_process.HasExited)
                {
                    await StopAsync();
                }
                    
                _process.Dispose();
                _process = null;
            }
            catch (Exception ex)
            {
                RaiseError("Cleanup failed", ErrorType.ProcessStop, ex);
            }
        }

        public void Dispose()
        {
            CleanupAsync().Wait();
        }
        #endregion
    }
} 
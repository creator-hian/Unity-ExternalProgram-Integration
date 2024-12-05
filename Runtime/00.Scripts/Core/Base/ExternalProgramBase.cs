using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Hian.ExternalProgram.Core.Communication;
using System.Threading;

namespace Hian.ExternalProgram.Core
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
        protected readonly ICommunicationProtocol _protocol;
        private bool _disposed = false;
        #endregion

        protected ExternalProgramBase(
            ProgramConfig config,
            ICommunicationProtocol protocol)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        #region IProcessManager Implementation
        public virtual string ProcessName => _config.ProcessName;
        public virtual string ExecutablePath => _config.ExecutablePath;
        public virtual string Arguments => _config.Arguments;
        public virtual bool IsRunning => _process != null && !_process.HasExited;

        public abstract Task<bool> StartAsync(CancellationToken cancellationToken = default);
        public abstract Task<bool> StopAsync();
        public abstract bool Start();
        public abstract bool Stop();

        public event Action<string> OnProcessOutput;
        public event Action<string> OnProcessError;
        public event Action<int> OnProcessExit;
        #endregion

        #region IExternalProgram Additional Implementation
        public abstract bool IsConnected { get; }
        public ICommunicationProtocol CommunicationProtocol => _protocol;

        public abstract Task<bool> ConnectAsync();
        public abstract Task DisconnectAsync();
        public abstract bool Connect();
        public abstract void Disconnect();

        public event Action<ProgramState> OnStateChanged;
        public event Action<ProgramError> OnError;

        public abstract Task SendCommandAsync(string command);
        public abstract Task<string> WaitForResponseAsync(string expectedPattern, TimeSpan? timeout = null);
        #endregion

        // 이벤트 발생을 위한 protected 메서드들
        protected virtual void RaiseStateChanged(ProgramState state)
        {
            OnStateChanged?.Invoke(state);
        }

        protected virtual void RaiseError(ProgramError error)
        {
            OnError?.Invoke(error);
        }

        protected virtual void RaiseProcessOutput(string output)
        {
            OnProcessOutput?.Invoke(output);
        }

        protected virtual void RaiseProcessError(string error)
        {
            OnProcessError?.Invoke(error);
        }

        protected virtual void RaiseProcessExit(int exitCode)
        {
            OnProcessExit?.Invoke(exitCode);
        }

        // ... 나머지 유틸리티 메서드들

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _protocol?.Dispose();
                    if (_process != null)
                    {
                        if (!_process.HasExited)
                        {
                            try
                            {
                                _process.Kill();
                                _process.WaitForExit();
                            }
                            catch (Exception ex)
                            {
                                UnityEngine.Debug.LogError($"Error during process cleanup: {ex.Message}");
                            }
                        }
                        _process.Dispose();
                        _process = null;
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ExternalProgramBase()
        {
            Dispose(false);
        }

        public virtual async Task<bool> RestartAsync(CancellationToken cancellationToken = default)
        {
            if (IsRunning)
            {
                await StopAsync();
            }
            return await StartAsync(cancellationToken);
        }
    }
} 
using System;
using System.Threading;
using System.Threading.Tasks;
using FAMOZ.ExternalProgram.Core;
using FAMOZ.ExternalProgram.Core.Communication;

namespace Mocks
{
    /// <summary>
    /// 외부 프로그램 테스트를 위한 Mock 구현체입니다.
    /// 실제 프로세스를 실행하지 않고 메모리상의 상태만 관리합니다.
    /// </summary>
    public class MockExternalProgram : ExternalProgramBase
    {
        private bool _isConnected;
        private bool _isRunning;
        private bool _disposed;

        public MockExternalProgram(
            ProgramConfig config,
            ICommunicationProtocol protocol,
            ILogger logger = null)
            : base(config, protocol, logger)
        {
            _isConnected = false;
            _isRunning = false;
        }

        public override bool IsConnected => _isConnected;
        public override bool IsRunning => _isRunning;

        #region Process Control
        public override async Task<bool> StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogWarning("Start operation was cancelled");
                    return false;
                }

                if (_isRunning)
                {
                    _logger?.LogWarning("Process is already running");
                    return false;
                }

                try
                {
                    await Task.Delay(100, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger?.LogWarning("Start operation was cancelled");
                    return false;
                }

                _isRunning = true;
                _currentState = ProgramState.Running;
                RaiseProcessOutput("Process started");
                RaiseStateChanged(_currentState);
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Start operation was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                var error = new ProgramError("Start failed", ErrorType.ProcessStart, ex);
                RaiseError(error);
                return false;
            }
        }

        public override async Task<bool> StopAsync()
        {
            try
            {
                if (!_isRunning)
                {
                    _logger?.LogWarning("Process is not running");
                    return false;
                }

                await Task.Delay(100); // 종료 시간 시뮬레이션
                _isRunning = false;
                _isConnected = false;
                _currentState = ProgramState.Stopped;
                RaiseProcessOutput("Process stopped");
                RaiseStateChanged(_currentState);
                RaiseProcessExit(0);
                return true;
            }
            catch (Exception ex)
            {
                var error = new ProgramError("Stop failed", ErrorType.ProcessStop, ex);
                RaiseError(error);
                return false;
            }
        }

        public override bool Start()
        {
            ThrowIfDisposed();

            if (_isRunning)
            {
                _logger?.LogWarning("Process is already running");
                return false;
            }

            _isRunning = true;
            _currentState = ProgramState.Running;
            RaiseProcessOutput("Process started");
            RaiseStateChanged(_currentState);
            return true;
        }

        public override bool Stop()
        {
            if (!_isRunning)
            {
                _logger?.LogWarning("Process is not running");
                return false;
            }

            _isRunning = false;
            _isConnected = false;
            _currentState = ProgramState.Stopped;
            RaiseProcessOutput("Process stopped");
            RaiseStateChanged(_currentState);
            RaiseProcessExit(0);
            return true;
        }
        #endregion

        #region Communication
        public override async Task<bool> ConnectAsync()
        {
            if (!_isRunning)
            {
                var error = new ProgramError("Cannot connect: Process not running", ErrorType.InvalidOperation);
                RaiseError(error);
                return false;
            }

            if (_isConnected)
            {
                _logger?.LogWarning("Already connected");
                return false;
            }

            await Task.Delay(50);
            _isConnected = true;
            RaiseProcessOutput("Connected to process");
            return true;
        }

        public override async Task DisconnectAsync()
        {
            if (_isConnected)
            {
                await Task.Delay(50); // 연결 해제 시간 시뮬레이션
                _isConnected = false;
                RaiseProcessOutput("Disconnected from process");
            }
        }

        public override bool Connect()
        {
            ThrowIfDisposed();

            if (!_isRunning)
            {
                var error = new ProgramError("Cannot connect: Process not running", ErrorType.InvalidOperation);
                RaiseError(error);
                return false;
            }

            if (_isConnected)
            {
                _logger?.LogWarning("Already connected");
                return false;
            }

            _isConnected = true;
            RaiseProcessOutput("Connected to process");
            return true;
        }

        public override void Disconnect()
        {
            if (_isConnected)
            {
                _isConnected = false;
                RaiseProcessOutput("Disconnected from process");
            }
        }

        public override async Task SendCommandAsync(string command)
        {
            try
            {
                if (!_isConnected)
                {
                    var error = new ProgramError("Cannot send command: Not connected", ErrorType.InvalidOperation);
                    RaiseError(error);
                    throw new InvalidOperationException(error.Message);
                }

                await Task.Delay(50);
                
                try
                {
                    await _protocol.SendAsync(System.Text.Encoding.UTF8.GetBytes(command));
                    RaiseProcessOutput($"Command sent: {command}");
                }
                catch (Exception ex)
                {
                    var error = new ProgramError("Communication failed", ErrorType.Communication, ex);
                    RaiseError(error);
                    throw;
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = new ProgramError("Send command failed", ErrorType.Communication, ex);
                RaiseError(error);
                throw;
            }
        }

        public override async Task<string> WaitForResponseAsync(string expectedPattern, TimeSpan? timeout = null)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected");

            if (timeout.HasValue)
            {
                // 타임아웃이 지정된 경우 즉시 타임아웃 발생
                throw new TimeoutException($"Response wait timed out after {timeout.Value.TotalMilliseconds}ms");
            }

            // 타임아웃이 없는 경우 정상 응답
            await Task.Delay(50);
            var response = $"MockResponse: {expectedPattern}";
            RaiseProcessOutput($"Response received: {response}");
            return response;
        }
        #endregion

        // 테스트를 위한 이벤트 발생 메서드들
        public void EmitProcessOutput(string output)
        {
            RaiseProcessOutput(output);
        }

        public void EmitProcessError(string error)
        {
            RaiseProcessError(error);
        }

        public void EmitProcessExit(int exitCode)
        {
            RaiseProcessExit(exitCode);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_isRunning)
                        Stop();
                    _isConnected = false;
                    _isRunning = false;
                }
                _disposed = true;
                base.Dispose(disposing);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MockExternalProgram));
            }
        }
    }
} 
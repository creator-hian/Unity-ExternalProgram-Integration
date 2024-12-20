using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hian.ExternalProgram.Core.Communication
{
    /// <summary>
    /// TCP 프로토콜을 사용한 통신을 구현하는 클래스입니다.
    /// 이 클래스는 비동기 작업과 스레드 안전을 보장합니다.
    /// </summary>
    public class TcpProtocol : ICommunicationProtocol, IDisposable
    {
        // 상수 정의
        private const int BufferSize = 8192; // 8KB 버퍼 크기
        private const int DefaultTimeout = 5000; // 5초 기본 타임아웃

        // 필드
        private readonly TcpClient _client;
        private readonly IPEndPoint _endPoint;
        private NetworkStream _stream;
        private readonly CancellationTokenSource _cts;
        private Task _receiveTask;
        private readonly object _lock = new object();
        private volatile bool _isDisposed;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public bool IsConnected
        {
            get
            {
                if (_isDisposed || _client == null)
                {
                    return false;
                }

                try
                {
                    return _client.Connected
                        && _stream?.CanRead == true
                        && _stream?.CanWrite == true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #region Events

        public event Action<byte[]> OnDataReceived;

        public event Action<Exception> OnError;

        public event Action OnConnected;

        public event Action OnDisconnected;

        public event Action<EDisconnectReason> OnConnectionLost;

        public event Action<ConnectionState> OnStateChanged;

        public event Action<int> OnConnectionAttempt;

        public event Action<TimeSpan> OnReconnecting;
        #endregion

        /// <summary>
        /// TCP 프로토콜 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="host">연결할 호스트 주소</param>
        /// <param name="port">연결할 포트 번호</param>
        public TcpProtocol(string host, int port)
        {
            _client = new TcpClient
            {
                ReceiveTimeout = DefaultTimeout,
                SendTimeout = DefaultTimeout,
            };
            _endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            _cts = new CancellationTokenSource();
        }

        // ICommunicationProtocol 이벤트 구현
        event Action<byte[]> ICommunicationProtocol.OnDataReceived
        {
            add => OnDataReceived += value;
            remove => OnDataReceived -= value;
        }

        event Action<Exception> ICommunicationProtocol.OnError
        {
            add => OnError += value;
            remove => OnError -= value;
        }

        public bool Connect()
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                try
                {
                    if (IsConnected)
                    {
                        return true;
                    }

                    OnStateChanged?.Invoke(ConnectionState.Connecting);
                    _client.Connect(_endPoint);
                    _stream = _client.GetStream();
                    StartReceiving();
                    OnConnected?.Invoke();
                    OnStateChanged?.Invoke(ConnectionState.Open);
                    return true;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                    OnStateChanged?.Invoke(ConnectionState.Closed);
                    return false;
                }
            }
        }

        public async Task<bool> ConnectAsync()
        {
            ThrowIfDisposed();
            // 타임아웃 처리를 위한 CancellationToken 설정
            using CancellationTokenSource timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using CancellationTokenSource linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, timeoutCts.Token);

            try
            {
                if (IsConnected)
                {
                    return true;
                }

                OnStateChanged?.Invoke(ConnectionState.Connecting);
                await _client.ConnectAsync(_endPoint.Address, _endPoint.Port).ConfigureAwait(false);
                _stream = _client.GetStream();
                StartReceiving();
                OnConnected?.Invoke();
                OnStateChanged?.Invoke(ConnectionState.Open);
                return true;
            }
            catch (OperationCanceledException)
            {
                OnConnectionLost?.Invoke(EDisconnectReason.Timeout);
                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                OnStateChanged?.Invoke(ConnectionState.Closed);
                return false;
            }
        }

        /// <summary>
        /// 비동기 데이터 수신을 시작합니다.
        /// </summary>
        private void StartReceiving()
        {
            _receiveTask = Task.Run(
                async () =>
                {
                    byte[] buffer = new byte[BufferSize];

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            int bytesRead = await _stream
                                .ReadAsync(buffer, 0, buffer.Length, _cts.Token)
                                .ConfigureAwait(false);
                            if (bytesRead > 0)
                            {
                                byte[] received = new byte[bytesRead];
                                Array.Copy(buffer, received, bytesRead);
                                OnDataReceived?.Invoke(received);
                            }
                            else
                            {
                                // 원격 종단에서 연결을 종료한 경우
                                OnConnectionLost?.Invoke(EDisconnectReason.Normal);
                                break;
                            }
                        }
                        catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
                        {
                            // 정상적인 취소 요청으로 인한 종료
                            break;
                        }
                        catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
                        {
                            // 예기치 않은 오류 발생
                            OnError?.Invoke(ex);
                            OnConnectionLost?.Invoke(EDisconnectReason.Unknown);
                            break;
                        }
                    }
                },
                _cts.Token
            );
        }

        /// <summary>
        /// 리소스를 해제합니다.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            lock (_lock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;

                try
                {
                    _cts.Cancel();

                    // 수신 태스크 완료 대기 (1초 타임아웃)
                    if (_receiveTask != null && !_receiveTask.IsCompleted)
                    {
                        _ = Task.WaitAny(_receiveTask, Task.Delay(1000));
                    }

                    _stream?.Dispose();
                    _client?.Dispose();
                    _cts.Dispose();
                    _sendLock.Dispose();

                    OnDisconnected?.Invoke();
                    OnStateChanged?.Invoke(ConnectionState.Closed);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }
            }
        }

        public void Disconnect()
        {
            ThrowIfDisposed();
            if (!IsConnected)
            {
                return;
            }

            lock (_lock)
            {
                try
                {
                    _cts.Cancel();
                    _client.Close();
                    OnDisconnected?.Invoke();
                    OnStateChanged?.Invoke(ConnectionState.Closed);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }
            }
        }

        public bool Send(byte[] data)
        {
            ThrowIfDisposed();
            if (!IsConnected || data == null)
            {
                return false;
            }

            try
            {
                _sendLock.Wait(_cts.Token);
                try
                {
                    _stream.Write(data, 0, data.Length);
                    return true;
                }
                finally
                {
                    _ = _sendLock.Release();
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }

        public byte[] Receive()
        {
            ThrowIfDisposed();
            if (!IsConnected)
            {
                return null;
            }

            try
            {
                byte[] buffer = new byte[BufferSize];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    byte[] received = new byte[bytesRead];
                    Array.Copy(buffer, received, bytesRead);
                    return received;
                }
                return null;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return null;
            }
        }

        Task<bool> ICommunicationProtocol.ConnectAsync()
        {
            return ConnectAsync();
        }

        public async Task DisconnectAsync()
        {
            ThrowIfDisposed();
            if (!IsConnected)
            {
                return;
            }

            try
            {
                await Task.Run(() =>
                    {
                        lock (_lock)
                        {
                            _cts.Cancel();
                            _client.Close();
                            OnDisconnected?.Invoke();
                            OnStateChanged?.Invoke(ConnectionState.Closed);
                        }
                    })
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        public async Task<bool> SendAsync(byte[] data)
        {
            ThrowIfDisposed();
            if (!IsConnected || data == null)
            {
                return false;
            }

            try
            {
                await _sendLock.WaitAsync(_cts.Token).ConfigureAwait(false);
                try
                {
                    await _stream
                        .WriteAsync(data, 0, data.Length, _cts.Token)
                        .ConfigureAwait(false);
                    return true;
                }
                finally
                {
                    _ = _sendLock.Release();
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }

        public async Task<byte[]> ReceiveAsync()
        {
            ThrowIfDisposed();
            if (!IsConnected)
            {
                return null;
            }

            try
            {
                byte[] buffer = new byte[BufferSize];
                int bytesRead = await _stream
                    .ReadAsync(buffer, 0, buffer.Length, _cts.Token)
                    .ConfigureAwait(false);
                if (bytesRead > 0)
                {
                    byte[] received = new byte[bytesRead];
                    Array.Copy(buffer, received, bytesRead);
                    return received;
                }
                return null;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return null;
            }
        }

        /// <summary>
        /// 객체가 이미 Dispose되었는지 확인하고, Dispose된 경우 예외를 발생시킵니다.
        /// </summary>
        /// <exception cref="ObjectDisposedException">객체가 이미 Dispose된 경우</exception>
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpProtocol));
            }
        }
    }
}

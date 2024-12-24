using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Hian.ExternalProgram.Core.Communication.Protocols.Serial
{
    /// <summary>
    /// 시리얼 통신을 구현하는 프로토콜 클래스입니다.
    /// </summary>
    public class SerialProtocol : ICommunicationProtocol, IDisposable
    {
        private readonly ISerialSettings _settings;
        private readonly SerialPort _serialPort;
        private readonly CancellationTokenSource _cts;
        private Task _receiveTask;
        private readonly object _lock = new object();
        private volatile bool _isDisposed;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private readonly ConcurrentQueue<byte[]> _receiveQueue = new ConcurrentQueue<byte[]>();
        private readonly AutoResetEvent _dataAvailable = new AutoResetEvent(false);

        public bool IsConnected => !_isDisposed && _serialPort?.IsOpen == true;

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
        /// SerialProtocol의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="portName">COM 포트 이름 (예: COM1)</param>
        /// <param name="settings">시리얼 포트 설정</param>
        public SerialProtocol(string portName, ISerialSettings settings = null)
        {
            _settings = settings ?? new SerialSettingsImpl();

            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = _settings.BaudRate,
                DataBits = _settings.DataBits,
                StopBits = _settings.StopBits,
                Parity = _settings.Parity,
                Handshake = _settings.Handshake,
                ReadTimeout = _settings.ReceiveTimeoutMs,
                WriteTimeout = _settings.SendTimeoutMs,
                ReadBufferSize = _settings.BufferSize,
                WriteBufferSize = _settings.BufferSize,
                DtrEnable = _settings.DtrEnable,
                RtsEnable = _settings.RtsEnable,
                DiscardNull = _settings.DiscardNull,
                ParityReplace = _settings.ParityReplace,
                BreakState = _settings.BreakState,
            };

            _cts = new CancellationTokenSource();
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
                    _serialPort.Open();
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
            return await Task.Run(() => Connect()).ConfigureAwait(false);
        }

        private void StartReceiving()
        {
            _receiveTask = Task.Run(
                async () =>
                {
                    byte[] buffer = new byte[_settings.BufferSize];

                    while (!_cts.Token.IsCancellationRequested && IsConnected)
                    {
                        try
                        {
                            if (_serialPort.BytesToRead > 0)
                            {
                                int bytesRead = await Task.Run(
                                        () => _serialPort.Read(buffer, 0, buffer.Length)
                                    )
                                    .ConfigureAwait(false);

                                if (bytesRead > 0)
                                {
                                    byte[] data = new byte[bytesRead];
                                    Buffer.BlockCopy(buffer, 0, data, 0, bytesRead);
                                    OnDataReceived?.Invoke(data);
                                }
                            }
                            else
                            {
                                await Task.Delay(1).ConfigureAwait(false);
                            }
                        }
                        catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
                        {
                            break;
                        }
                        catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
                        {
                            OnError?.Invoke(ex);
                            await HandleConnectionLostAsync(EDisconnectReason.Unknown)
                                .ConfigureAwait(false);
                            break;
                        }
                    }
                },
                _cts.Token
            );
        }

        private async Task HandleConnectionLostAsync(EDisconnectReason reason)
        {
            OnConnectionLost?.Invoke(reason);
            OnStateChanged?.Invoke(ConnectionState.Connecting);

            if (await TryReconnectAsync().ConfigureAwait(false))
            {
                return;
            }

            OnStateChanged?.Invoke(ConnectionState.Closed);
        }

        private async Task<bool> TryReconnectAsync()
        {
            for (int attempt = 1; attempt <= _settings.MaxReconnectAttempts; attempt++)
            {
                try
                {
                    OnConnectionAttempt?.Invoke(attempt);
                    TimeSpan delay = TimeSpan.FromMilliseconds(
                        _settings.ReconnectDelayMs * attempt
                    );
                    OnReconnecting?.Invoke(delay);

                    await Task.Delay(delay).ConfigureAwait(false);

                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }

                    _serialPort.Open();
                    StartReceiving();
                    OnConnected?.Invoke();
                    OnStateChanged?.Invoke(ConnectionState.Open);
                    return true;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }
            }

            return false;
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
                    _serialPort.Close();
                    OnDisconnected?.Invoke();
                    OnStateChanged?.Invoke(ConnectionState.Closed);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }
            }
        }

        public async Task DisconnectAsync()
        {
            await Task.Run(() => Disconnect()).ConfigureAwait(false);
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
                    _serialPort.Write(data, 0, data.Length);
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
                    await Task.Run(() => _serialPort.Write(data, 0, data.Length))
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

        public byte[] Receive()
        {
            ThrowIfDisposed();
            if (!IsConnected)
            {
                return null;
            }

            try
            {
                if (_receiveQueue.TryDequeue(out byte[] data))
                {
                    return data;
                }

                int bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = _serialPort.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        byte[] result = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, result, 0, bytesRead);
                        return result;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return null;
            }
        }

        public async Task<byte[]> ReceiveAsync()
        {
            return await Task.Run(() => Receive()).ConfigureAwait(false);
        }

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

                    if (_receiveTask != null && !_receiveTask.IsCompleted)
                    {
                        _ = Task.WaitAny(_receiveTask, Task.Delay(1000));
                    }

                    _serialPort?.Close();
                    _serialPort?.Dispose();
                    _cts.Dispose();
                    _sendLock.Dispose();
                    _dataAvailable.Dispose();

                    OnDisconnected?.Invoke();
                    OnStateChanged?.Invoke(ConnectionState.Closed);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SerialProtocol));
            }
        }

        /// <summary>
        /// 사용 가능한 시리얼 포트 목록을 반환합니다.
        /// </summary>
        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }
    }

    public class SerialSettings
    {
        public int DataBits { get; set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Parity Parity { get; set; } = Parity.None;
        public Handshake Handshake { get; set; } = Handshake.None;
        public int ReadTimeout { get; set; } = DefaultTimeout;
        public int WriteTimeout { get; set; } = DefaultTimeout;
        public int BufferSize { get; set; } = DefaultBufferSize;

        private const int DefaultBufferSize = 4096;
        private const int DefaultTimeout = 1000;

        public static SerialSettings Default => new SerialSettings();
    }
}

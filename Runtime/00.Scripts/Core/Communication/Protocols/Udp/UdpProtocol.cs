using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hian.ExternalProgram.Core.Communication.Protocols.Udp
{
    /// <summary>
    /// UDP 프로토콜을 사용한 통신을 구현하는 클래스입니다.
    /// 이 클래스는 비동기 작업과 스레드 안전을 보장합니다.
    /// </summary>
    public class UdpProtocol : ICommunicationProtocol, IDisposable
    {
        private readonly IUdpSettings _settings;
        private readonly UdpClient _client;
        private readonly IPEndPoint _remoteEndPoint;
        private readonly CancellationTokenSource _cts;
        private Task _receiveTask;
        private readonly object _lock = new object();
        private volatile bool _isDisposed;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private bool _isRunning;
        private long _nextSequenceNumber = 0;
        private readonly ConcurrentDictionary<long, PendingPacket> _pendingPackets =
            new ConcurrentDictionary<long, PendingPacket>();
        private readonly ConcurrentDictionary<long, byte[][]> _receivingPackets =
            new ConcurrentDictionary<long, byte[][]>();

        public bool IsConnected => !_isDisposed && _isRunning;

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

        public UdpProtocol(string host, int port, IUdpSettings settings = null)
        {
            _settings = settings ?? new UdpSettingsImpl();
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            _client = new UdpClient();

            // UDP 설정 적용
            if (_settings.EnableMulticast)
            {
                _client.JoinMulticastGroup(IPAddress.Parse(_settings.MulticastGroup));
                _client.MulticastLoopback = _settings.MulticastLoopback;
            }

            _client.DontFragment = _settings.DontFragment;
            _client.Ttl = (short)_settings.TimeToLive;
            _client.EnableBroadcast = _settings.Broadcast;

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
                    _client.Connect(_remoteEndPoint);
                    _isRunning = true;
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
            using CancellationTokenSource timeoutCts = new CancellationTokenSource(
                _settings.ConnectionTimeoutMs
            );
            using CancellationTokenSource linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, timeoutCts.Token);

            try
            {
                if (IsConnected)
                {
                    return true;
                }

                OnStateChanged?.Invoke(ConnectionState.Connecting);
                await Task.Run(() => _client.Connect(_remoteEndPoint), linkedCts.Token);
                _isRunning = true;
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

        private void StartReceiving()
        {
            _receiveTask = Task.Run(
                async () =>
                {
                    while (!_cts.Token.IsCancellationRequested && _isRunning)
                    {
                        try
                        {
                            UdpReceiveResult result = await _client
                                .ReceiveAsync()
                                .ConfigureAwait(false);
                            if (result.Buffer.Length > 0)
                            {
                                if (result.Buffer.Length == 12) // ACK 패킷
                                {
                                    ProcessAck(result.Buffer);
                                }
                                else // 데이터 패킷
                                {
                                    ProcessReceivedData(result.Buffer);
                                }
                            }
                        }
                        catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
                        {
                            break;
                        }
                        catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
                        {
                            OnError?.Invoke(ex);
                            OnConnectionLost?.Invoke(EDisconnectReason.Unknown);
                            break;
                        }
                    }
                },
                _cts.Token
            );

            // 재전송 타이머 시작
            StartRetransmissionTimer();
        }

        private void StartRetransmissionTimer()
        {
            _ = Task.Run(
                async () =>
                {
                    while (!_cts.Token.IsCancellationRequested && _isRunning)
                    {
                        await Task.Delay(100).ConfigureAwait(false); // 100ms 간격으로 체크

                        DateTime now = DateTime.UtcNow;
                        foreach (PendingPacket packet in _pendingPackets.Values)
                        {
                            if (
                                (now - packet.LastSentTime).TotalMilliseconds
                                >= _settings.AckTimeoutMs
                            )
                            {
                                if (packet.RetryCount >= _settings.MaxRetransmissions)
                                {
                                    _ = packet.CompletionSource.TrySetResult(false);
                                    continue;
                                }

                                packet.RetryCount++;
                                packet.LastSentTime = now;

                                // 미확인 청크 재전송
                                for (int i = 0; i < packet.AckReceived.Length; i++)
                                {
                                    if (!packet.AckReceived[i])
                                    {
                                        await SendChunkAsync(packet.Chunks[i])
                                            .ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                },
                _cts.Token
            );
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
                    _isRunning = false;
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
                            _isRunning = false;
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

        private async Task<bool> SendWithReliabilityAsync(byte[] data)
        {
            if (!_settings.EnableReliableDelivery)
            {
                return await SendAsync(data).ConfigureAwait(false);
            }

            long sequenceNumber = Interlocked.Increment(ref _nextSequenceNumber);
            int maxDataSize = _settings.MaxPacketSize - 12; // 헤더 크기(12바이트) 제외
            int totalChunks = (int)Math.Ceiling((double)data.Length / maxDataSize);
            byte[][] chunks = new byte[totalChunks][];

            // 데이터를 청크로 분할
            for (int i = 0; i < totalChunks; i++)
            {
                int offset = i * maxDataSize;
                int size = Math.Min(maxDataSize, data.Length - offset);
                chunks[i] = new byte[size + 12];

                // 헤더 정보 추가
                Buffer.BlockCopy(BitConverter.GetBytes(sequenceNumber), 0, chunks[i], 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(totalChunks), 0, chunks[i], 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(i), 0, chunks[i], 8, 4);
                Buffer.BlockCopy(data, offset, chunks[i], 12, size);
            }

            PendingPacket packet = new PendingPacket
            {
                Chunks = chunks,
                AckReceived = new bool[totalChunks],
                RetryCount = 0,
                CompletionSource = new TaskCompletionSource<bool>(),
                LastSentTime = DateTime.UtcNow,
            };

            _ = _pendingPackets.TryAdd(sequenceNumber, packet);

            try
            {
                // 모든 청크 전송
                for (int i = 0; i < totalChunks; i++)
                {
                    await SendChunkAsync(chunks[i]).ConfigureAwait(false);
                }

                // 완료 대기
                using CancellationTokenSource cts = new CancellationTokenSource(
                    _settings.AckTimeoutMs * (_settings.MaxRetransmissions + 1)
                );
                using (cts.Token.Register(() => packet.CompletionSource.TrySetResult(false)))
                {
                    return await packet.CompletionSource.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                _ = _pendingPackets.TryRemove(sequenceNumber, out _);
            }
        }

        private async Task SendChunkAsync(byte[] chunk)
        {
            try
            {
                await _sendLock.WaitAsync(_cts.Token).ConfigureAwait(false);
                try
                {
                    _ = await _client.SendAsync(chunk, chunk.Length).ConfigureAwait(false);
                }
                finally
                {
                    _ = _sendLock.Release();
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                throw;
            }
        }

        private void ProcessReceivedData(byte[] data)
        {
            const int HeaderSize = 12; // 시퀀스 번호(4) + 총 청크 수(4) + 청크 인덱스(4)
            if (data.Length < HeaderSize)
            {
                return;
            }

            long sequenceNumber = BitConverter.ToInt64(data, 0);
            int totalChunks = BitConverter.ToInt32(data, 4);
            int chunkIndex = BitConverter.ToInt32(data, 8);

            // ACK 전송
            _ = SendAckAsync(sequenceNumber, chunkIndex).ConfigureAwait(false);

            if (!_receivingPackets.TryGetValue(sequenceNumber, out byte[][] chunks))
            {
                chunks = new byte[totalChunks][];
                _ = _receivingPackets.TryAdd(sequenceNumber, chunks);
            }

            // 청크 데이터 저장
            byte[] chunkData = new byte[data.Length - HeaderSize];
            Buffer.BlockCopy(data, HeaderSize, chunkData, 0, chunkData.Length);
            chunks[chunkIndex] = chunkData;

            // 모든 청크가 수신되었는지 확인
            if (chunks.All(static chunk => chunk != null))
            {
                // 전체 데이터 조합
                int totalSize = chunks.Sum(static chunk => chunk.Length);
                byte[] completeData = new byte[totalSize];
                int offset = 0;
                foreach (byte[] chunk in chunks)
                {
                    Buffer.BlockCopy(chunk, 0, completeData, offset, chunk.Length);
                    offset += chunk.Length;
                }

                _ = _receivingPackets.TryRemove(sequenceNumber, out _);
                OnDataReceived?.Invoke(completeData);
            }
        }

        private async Task SendAckAsync(long sequenceNumber, int chunkIndex)
        {
            byte[] ackData = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(sequenceNumber), 0, ackData, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(chunkIndex), 0, ackData, 8, 4);

            try
            {
                _ = await _client.SendAsync(ackData, ackData.Length).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        private void ProcessAck(byte[] ackData)
        {
            if (ackData.Length != 12)
            {
                return;
            }

            long sequenceNumber = BitConverter.ToInt64(ackData, 0);
            int chunkIndex = BitConverter.ToInt32(ackData, 8);

            if (_pendingPackets.TryGetValue(sequenceNumber, out PendingPacket packet))
            {
                packet.AckReceived[chunkIndex] = true;

                // 모든 청크가 확인되었는지 검사
                if (packet.AckReceived.All(static ack => ack))
                {
                    _ = packet.CompletionSource.TrySetResult(true);
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
                    int bytesSent = _client.Send(data, data.Length);
                    return bytesSent == data.Length;
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
                return await SendWithReliabilityAsync(data).ConfigureAwait(false);
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
                IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
                return _client.Receive(ref remoteEp);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return null;
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
                UdpReceiveResult result = await _client.ReceiveAsync().ConfigureAwait(false);
                return result.Buffer;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return null;
            }
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

                    _client?.Close();
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

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpProtocol));
            }
        }

        private class PendingPacket
        {
            public byte[][] Chunks { get; set; }
            public bool[] AckReceived { get; set; }
            public int RetryCount { get; set; }
            public TaskCompletionSource<bool> CompletionSource { get; set; }
            public DateTime LastSentTime { get; set; }
        }
    }
}

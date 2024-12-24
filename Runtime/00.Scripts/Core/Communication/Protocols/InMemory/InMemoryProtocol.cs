using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hian.ExternalProgram.Core.Communication.Protocols.InMemory
{
    /// <summary>
    /// 프로세스 내 메모리 통신을 구현하는 프로토콜 클래스입니다.
    /// 주로 테스트나 같은 프로세스 내 통신에 사용됩니다.
    /// </summary>
    public class InMemoryProtocol : ICommunicationProtocol, IDisposable
    {
        private static readonly ConcurrentDictionary<string, ConcurrentQueue<byte[]>> _channels =
            new ConcurrentDictionary<string, ConcurrentQueue<byte[]>>();

        private readonly string _channelId;
        private readonly IInMemorySettings _settings;
        private readonly CancellationTokenSource _cts;
        private Task _receiveTask;
        private volatile bool _isDisposed;
        private volatile bool _isConnected;
        private readonly object _lock = new object();
        private readonly AutoResetEvent _dataAvailable = new AutoResetEvent(false);
        private readonly ConcurrentQueue<PriorityMessage> _priorityQueue;
        private readonly Timer _batchTimer;
        private readonly ConcurrentQueue<byte[]> _batchQueue;
        private DateTime _lastMessageTime;

        public bool IsConnected => !_isDisposed && _isConnected;

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

        public InMemoryProtocol(string channelId, IInMemorySettings settings = null)
        {
            _channelId = channelId;
            _settings = settings ?? new InMemorySettingsImpl();
            _cts = new CancellationTokenSource();

            if (_settings.EnablePriority)
            {
                _priorityQueue = new ConcurrentQueue<PriorityMessage>();
            }

            if (_settings.EnableBatching)
            {
                _batchQueue = new ConcurrentQueue<byte[]>();
                _batchTimer = new Timer(
                    ProcessBatch,
                    null,
                    _settings.BatchTimeout,
                    _settings.BatchTimeout
                );
            }

            _lastMessageTime = DateTime.UtcNow;
        }

        private void ProcessBatch(object state)
        {
            if (!_settings.EnableBatching || !IsConnected)
            {
                return;
            }

            List<byte[]> batchData = new List<byte[]>();
            while (_batchQueue.TryDequeue(out byte[] data) && batchData.Count < _settings.BatchSize)
            {
                batchData.Add(data);
            }

            if (batchData.Count > 0)
            {
                // 배치 데이터를 하나의 메시지로 결합
                int totalLength = batchData.Sum(static d => d.Length);
                byte[] combinedData = new byte[totalLength];
                int offset = 0;
                foreach (byte[] data in batchData)
                {
                    Buffer.BlockCopy(data, 0, combinedData, offset, data.Length);
                    offset += data.Length;
                }

                OnDataReceived?.Invoke(combinedData);
            }
        }

        private void StartReceiving()
        {
            _receiveTask = Task.Run(
                async () =>
                {
                    while (!_cts.Token.IsCancellationRequested && _isConnected)
                    {
                        try
                        {
                            _ = await Task.Run(() => _dataAvailable.WaitOne(100), _cts.Token);

                            if (
                                _channels.TryGetValue(_channelId, out ConcurrentQueue<byte[]> queue)
                            )
                            {
                                if (_settings.EnableMessageExpiry)
                                {
                                    // 만료된 메시지 제거
                                    while (queue.TryPeek(out _))
                                    {
                                        if (
                                            (DateTime.UtcNow - _lastMessageTime)
                                            > _settings.MessageTtl
                                        )
                                        {
                                            _ = queue.TryDequeue(out _);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                if (_settings.EnablePriority)
                                {
                                    ProcessPriorityMessages();
                                }
                                else if (_settings.EnableBatching)
                                {
                                    while (queue.TryDequeue(out byte[] data))
                                    {
                                        _batchQueue.Enqueue(data);
                                    }
                                }
                                else
                                {
                                    while (queue.TryDequeue(out byte[] data))
                                    {
                                        OnDataReceived?.Invoke(data);
                                    }
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
        }

        private void ProcessPriorityMessages()
        {
            if (!_settings.EnablePriority || _priorityQueue == null)
            {
                return;
            }

            List<PriorityMessage> messages = new List<PriorityMessage>();
            while (_priorityQueue.TryDequeue(out PriorityMessage msg))
            {
                messages.Add(msg);
            }

            // 우선순위별로 정렬
            foreach (PriorityMessage msg in messages.OrderByDescending(static m => m.Priority))
            {
                OnDataReceived?.Invoke(msg.Data);
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
                    _batchTimer?.Dispose();

                    if (_receiveTask != null && !_receiveTask.IsCompleted)
                    {
                        _ = Task.WaitAny(_receiveTask, Task.Delay(1000));
                    }

                    _ = _channels.TryRemove(_channelId, out _);
                    _cts.Dispose();
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

        event Action<ConnectionState> ICommunicationProtocol.OnStateChanged
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
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
                    _ = _channels.TryAdd(_channelId, new ConcurrentQueue<byte[]>());
                    _isConnected = true;
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
                    _isConnected = false;
                    _ = _channels.TryRemove(_channelId, out _);
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
                if (_channels.TryGetValue(_channelId, out ConcurrentQueue<byte[]> queue))
                {
                    queue.Enqueue(data);
                    _ = _dataAvailable.Set();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }

        public async Task<bool> SendAsync(byte[] data)
        {
            return await Task.Run(() => Send(data)).ConfigureAwait(false);
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
                if (_channels.TryGetValue(_channelId, out ConcurrentQueue<byte[]> queue))
                {
                    if (queue.TryDequeue(out byte[] data))
                    {
                        return data;
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

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(InMemoryProtocol));
            }
        }

        private class PriorityMessage
        {
            public byte[] Data { get; set; }
            public int Priority { get; set; }
        }
    }
}

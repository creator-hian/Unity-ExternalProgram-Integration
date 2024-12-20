using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hian.ExternalProgram.Core.Communication
{
    public class TcpProtocol : ICommunicationProtocol
    {
        private readonly TcpClient _client;
        private readonly IPEndPoint _endPoint;
        private NetworkStream _stream;
        private readonly CancellationTokenSource _cts;
        private Task _receiveTask;

        public bool IsConnected => _client?.Connected ?? false;

        public event Action<byte[]> OnDataReceived;
        public event Action<Exception> OnError;

        public TcpProtocol(string host, int port)
        {
            _client = new TcpClient();
            _endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            _cts = new CancellationTokenSource();
        }

        event Action<byte[]> ICommunicationProtocol.OnDataReceived
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event Action<Exception> ICommunicationProtocol.OnError
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public bool Connect()
        {
            try
            {
                _client.Connect(_endPoint);
                _stream = _client.GetStream();
                StartReceiving();
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                await _client.ConnectAsync(_endPoint.Address, _endPoint.Port);
                _stream = _client.GetStream();
                StartReceiving();
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }

        private void StartReceiving()
        {
            _receiveTask = Task.Run(
                async () =>
                {
                    byte[] buffer = new byte[4096];

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                byte[] received = new byte[bytesRead];
                                Array.Copy(buffer, received, bytesRead);
                                OnDataReceived?.Invoke(received);
                            }
                        }
                        catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
                        {
                            OnError?.Invoke(ex);
                        }
                    }
                },
                _cts.Token
            );
        }

        // 나머지 메서드 구현...

        public void Dispose()
        {
            _cts.Cancel();
            _receiveTask?.Wait();
            _stream?.Dispose();
            _client?.Dispose();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public bool Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] Receive()
        {
            throw new NotImplementedException();
        }

        Task<bool> ICommunicationProtocol.ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendAsync(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReceiveAsync()
        {
            throw new NotImplementedException();
        }
    }
}

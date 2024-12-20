using System;
using System.Data;
using System.Threading.Tasks;
using Hian.ExternalProgram.Core.Communication;

namespace Hian.ExternalProgram.Tests.Editor.Mocks
{
    public class MockCommunicationProtocol : ICommunicationProtocol
    {
        private bool _isConnected;
        private Exception _simulatedError;

        public bool IsConnected => _isConnected;

        public event Action<byte[]> OnDataReceived;
        public event Action<Exception> OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<EDisconnectReason> OnConnectionLost;
        public event Action<ConnectionState> OnStateChanged;
        public event Action<int> OnConnectionAttempt;
        public event Action<TimeSpan> OnReconnecting;

        public bool Connect()
        {
            _isConnected = true;
            return true;
        }

        public Task<bool> ConnectAsync()
        {
            return Task.FromResult(Connect());
        }

        public void Disconnect()
        {
            _isConnected = false;
        }

        public Task DisconnectAsync()
        {
            Disconnect();
            return Task.CompletedTask;
        }

        public byte[] Receive()
        {
            return new byte[] { 1, 2, 3 };
        }

        public Task<byte[]> ReceiveAsync()
        {
            return Task.FromResult(Receive());
        }

        public bool Send(byte[] data)
        {
            if (_simulatedError != null)
            {
                Exception error = _simulatedError;
                _simulatedError = null;
                throw error;
            }

            OnDataReceived?.Invoke(data);
            return true;
        }

        public async Task<bool> SendAsync(byte[] data)
        {
            if (_simulatedError != null)
            {
                Exception error = _simulatedError;
                _simulatedError = null;
                OnError?.Invoke(error);
                throw error;
            }

            await Task.Delay(50);
            OnDataReceived?.Invoke(data);
            return true;
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void SimulateError(Exception error)
        {
            _simulatedError = error;
            OnError?.Invoke(error);
        }
    }
}

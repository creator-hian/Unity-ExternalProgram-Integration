using System;
using System.Threading.Tasks;

namespace FAMOZ.ExternalProgram.Core.Communication
{
    public interface ICommunicationProtocol : IDisposable
    {
        bool IsConnected { get; }
        
        // 동기 메서드
        bool Connect();
        void Disconnect();
        bool Send(byte[] data);
        byte[] Receive();
        
        // 비동기 메서드
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        Task<bool> SendAsync(byte[] data);
        Task<byte[]> ReceiveAsync();
        
        // 이벤트
        event Action<byte[]> OnDataReceived;
        event Action<Exception> OnError;
    }
}

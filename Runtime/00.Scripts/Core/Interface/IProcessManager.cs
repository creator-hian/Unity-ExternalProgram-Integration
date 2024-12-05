using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hian.ExternalProgram.Core
{
    /// <summary>
    /// 기본 프로세스 관리를 위한 인터페이스입니다.
    /// </summary>
    public interface IProcessManager : IDisposable
    {
        string ProcessName { get; }
        string ExecutablePath { get; }
        string Arguments { get; }
        bool IsRunning { get; }
        
        Task<bool> StartAsync(CancellationToken cancellationToken = default);
        Task<bool> StopAsync();
        Task<bool> RestartAsync(CancellationToken cancellationToken = default);
        
        bool Start();
        bool Stop();
        
        event Action<string> OnProcessOutput;
        event Action<string> OnProcessError;
        event Action<int> OnProcessExit;
    }
}

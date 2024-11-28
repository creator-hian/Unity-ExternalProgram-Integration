using System;
using System.Threading.Tasks;
using FAMOZ.ExternalProgram.Core;

/// <summary>
/// 외부 프로그램 테스트를 위한 Mock 구현체입니다.
/// 실제 프로세스를 실행하지 않고 메모리상의 상태만 관리합니다.
/// </summary>
public class MockExternalProgram : ExternalProgramBase
{
    private bool _isConnected;

    public MockExternalProgram(ProgramConfig config) : base(config)
    {
        _isConnected = false;
    }

    public override bool IsConnected => _isConnected;

    /// <summary>
    /// 프로그램 시작을 시뮬레이션합니다.
    /// 상태를 Running으로 변경하고 연결 상태를 true로 설정합니다.
    /// </summary>
    public override Task StartAsync()
    {
        UpdateState(ProgramState.Running);
        _isConnected = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 프로그램 정지를 시뮬레이션합니다.
    /// 상태를 Stopped로 변경하고 연결 상태를 false로 설정합니다.
    /// </summary>
    public override Task StopAsync()
    {
        UpdateState(ProgramState.Stopped);
        _isConnected = false;
        return Task.CompletedTask;
    }

    public override Task<bool> IsRunningAsync()
    {
        return Task.FromResult(State == ProgramState.Running);
    }

    /// <summary>
    /// 연결되지 않은 상태에서의 명령 전송을 시뮬레이션합니다.
    /// 연결되지 않은 경우 InvalidOperation 에러를 발생시킵니다.
    /// </summary>
    public override Task SendCommandAsync(string command)
    {
        if (!IsConnected)
        {
            // 에러 이벤트 발생
            RaiseError(
                "Cannot send command: Not connected", 
                ErrorType.InvalidOperation
            );
            throw new InvalidOperationException("Not connected");
        }
        return Task.CompletedTask;
    }

    public override Task<string> WaitForResponseAsync(string expectedPattern, TimeSpan? timeout = null)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected");
        return Task.FromResult("MockResponse");
    }
}

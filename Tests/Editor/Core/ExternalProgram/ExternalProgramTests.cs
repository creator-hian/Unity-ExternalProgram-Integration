using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hian.ExternalProgram.Core;
using Hian.ExternalProgram.Tests.Editor.Core;
using Hian.ExternalProgram.Tests.Editor.Mocks;
using Mocks;
using NUnit.Framework;

namespace ExternalProgram
{
    [TestFixture]
    [Timeout(TestConstants.Timeouts.DEFAULT_TEST_TIMEOUT)]
    public class ExternalProgramTests
    {
        private MockExternalProgram _program;
        private MockCommunicationProtocol _protocol;
        private ProgramConfig _config;

        [SetUp]
        public void Setup()
        {
            _protocol = new MockCommunicationProtocol();
            _config = new ProgramConfig(
                processName: "TestProcess",
                executablePath: "test.exe",
                arguments: "-test",
                portNumber: 12345
            );
            _program = new MockExternalProgram(_config, _protocol);
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task Lifecycle_ShouldFollowExpectedStates()
        {
            // Initial state
            Assert.That(_program.IsRunning, Is.False);
            Assert.That(_program.IsConnected, Is.False);

            // Start
            bool startResult = await _program.StartAsync();
            Assert.That(startResult, Is.True);
            Assert.That(_program.IsRunning, Is.True);

            // Connect
            bool connectResult = await _program.ConnectAsync();
            Assert.That(connectResult, Is.True);
            Assert.That(_program.IsConnected, Is.True);

            // Stop
            bool stopResult = await _program.StopAsync();
            Assert.That(stopResult, Is.True);
            Assert.That(_program.IsRunning, Is.False);
            Assert.That(_program.IsConnected, Is.False);
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task Events_ShouldFollowCorrectSequence()
        {
            List<ProgramState> stateChanges = new List<ProgramState>();
            List<string> outputs = new List<string>();

            _program.OnStateChanged += state => stateChanges.Add(state);
            _program.OnProcessOutput += output => outputs.Add(output);

            // 시작
            _ = await _program.StartAsync();
            Assert.That(stateChanges, Is.EqualTo(new[] { ProgramState.Running }));

            // 연결
            _ = await _program.ConnectAsync();
            Assert.That(outputs, Has.Some.Contains("Connected to process"));

            // 명령 전송
            await _program.SendCommandAsync("test");
            Assert.That(outputs, Has.Some.Contains("Command sent: test"));

            // 연결 해제
            await _program.DisconnectAsync();
            Assert.That(outputs, Has.Some.Contains("Disconnected from process"));

            // 종료
            _ = await _program.StopAsync();
            Assert.That(
                stateChanges,
                Is.EqualTo(new[] { ProgramState.Running, ProgramState.Stopped })
            );

            // 전체 출력 순서 확인
            Assert.That(outputs, Has.Count.GreaterThan(0));
            Assert.That(
                outputs.IndexOf("Process started") < outputs.IndexOf("Connected to process")
            );
            Assert.That(
                outputs.IndexOf("Connected to process") < outputs.IndexOf("Command sent: test")
            );
            Assert.That(
                outputs.IndexOf("Command sent: test") < outputs.IndexOf("Disconnected from process")
            );
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task InvalidStateTransitions_ShouldFail()
        {
            // 시작하지 않은 상태에서 연결 시도
            bool connectResult = await _program.ConnectAsync();
            Assert.That(connectResult, Is.False);
            Assert.That(_program.IsConnected, Is.False);

            // 시작
            _ = await _program.StartAsync();

            // 이미 실행 중일 때 다시 시작 시도
            bool startResult = await _program.StartAsync();
            Assert.That(startResult, Is.False);

            // 연결
            _ = await _program.ConnectAsync();

            // 이미 연결된 상태에서 다시 연결 시도
            connectResult = await _program.ConnectAsync();
            Assert.That(connectResult, Is.False);

            // 연결 해제 없이 종료
            bool stopResult = await _program.StopAsync();
            Assert.That(stopResult, Is.True);
            Assert.That(_program.IsConnected, Is.False, "연결이 자동으로 해제되어야 함");
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task Communication_WhenConnected_ShouldWork()
        {
            _ = await _program.StartAsync();
            _ = await _program.ConnectAsync();

            string command = "test_command";
            await _program.SendCommandAsync(command);
            string response = await _program.WaitForResponseAsync(command);

            Assert.That(response, Does.Contain(command));
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task StartAsync_WithCancellation_ShouldCancelOperation()
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            bool result = await _program.StartAsync(cts.Token);

            Assert.That(result, Is.False, "Operation should return false when cancelled");
            Assert.That(
                _program.IsRunning,
                Is.False,
                "Program should not be running after cancellation"
            );
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task StopAsync_WhenNotRunning_ShouldReturnFalse()
        {
            bool result = await _program.StopAsync();

            Assert.That(result, Is.False);
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task RestartAsync_ShouldResetState()
        {
            // Arrange
            _ = await _program.StartAsync();
            _ = await _program.ConnectAsync();

            // Act
            bool result = await _program.RestartAsync();

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_program.IsRunning, Is.True);
            Assert.That(_program.IsConnected, Is.False);
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task WaitForResponse_WithTimeout_ShouldThrow()
        {
            _ = await _program.StartAsync();
            _ = await _program.ConnectAsync();

            TimeSpan timeout = TimeSpan.FromMilliseconds(50);
            TimeoutException ex = Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                _ = await _program.WaitForResponseAsync("test", timeout);
            });

            Assert.That(ex.Message, Does.Contain("timed out"));
            Assert.That(ex.Message, Does.Contain("50ms")); // 타임아웃 값 확인
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task MultipleCommands_ShouldExecuteInOrder()
        {
            _ = await _program.StartAsync();
            _ = await _program.ConnectAsync();

            string[] commands = new[] { "cmd1", "cmd2", "cmd3" };
            List<string> responses = new List<string>();

            foreach (string cmd in commands)
            {
                await _program.SendCommandAsync(cmd);
                string response = await _program.WaitForResponseAsync(cmd);
                responses.Add(response);
            }

            Assert.That(responses.Count, Is.EqualTo(commands.Length));
            for (int i = 0; i < commands.Length; i++)
            {
                Assert.That(responses[i], Does.Contain(commands[i]));
            }
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task NetworkError_ShouldRaiseErrorEvent()
        {
            ProgramError caughtError = null;
            _program.OnError += error => caughtError = error;

            _ = await _program.StartAsync();
            _ = await _program.ConnectAsync();

            // 통신 프로토콜에 에러 발생 시뮬레이션
            Exception exception = new Exception("Network error");
            _protocol.SimulateError(exception);

            try
            {
                await _program.SendCommandAsync("test");
            }
            catch
            {
                // 예외는 무시 (에러 이벤트 확인이 목적)
            }

            Assert.That(caughtError, Is.Not.Null);
            Assert.That(caughtError.Type, Is.EqualTo(ErrorType.Communication));
            Assert.That(caughtError.Exception, Is.EqualTo(exception));
        }

        [TearDown]
        public void TearDown()
        {
            _program?.Dispose();
        }
    }
}

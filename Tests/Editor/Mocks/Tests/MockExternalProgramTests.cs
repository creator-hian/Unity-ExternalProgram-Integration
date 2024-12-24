using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hian.ExternalProgram.Core;
using Hian.ExternalProgram.Tests.Editor.Core;
using Hian.ExternalProgram.Tests.Editor.Mocks;
using NUnit.Framework;

namespace Mocks
{
    [TestFixture]
    [Timeout(TestConstants.Timeouts.DEFAULT_TEST_TIMEOUT)]
    public class MockExternalProgramTests
    {
        private MockExternalProgram _program;
        private MockCommunicationProtocol _protocol;

        [SetUp]
        public void Setup()
        {
            _protocol = new MockCommunicationProtocol();
            ProgramConfig config = new ProgramConfig(
                processName: "TestProcess",
                executablePath: "test.exe",
                arguments: "-test"
            );
            _program = new MockExternalProgram(config, _protocol);
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task StateTransitions_ShouldFollowExpectedSequence()
        {
            List<ProgramState> stateChanges = new List<ProgramState>();
            _program.OnStateChanged += state => stateChanges.Add(state);

            _ = await _program.StartAsync();
            _ = await _program.ConnectAsync();
            _ = await _program.StopAsync();

            Assert.That(
                stateChanges,
                Is.EqualTo(new[] { ProgramState.Running, ProgramState.Stopped })
            );
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task ErrorHandling_ShouldRaiseErrorEvent()
        {
            ProgramError caughtError = null;
            _program.OnError += error => caughtError = error;

            _ = await _program.StartAsync();

            InvalidOperationException exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _program.SendCommandAsync("test")
            );
            await Task.Delay(100);

            Assert.That(caughtError, Is.Not.Null);
            Assert.That(caughtError.Type, Is.EqualTo(ErrorType.InvalidOperation));
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task ProcessOutput_ShouldBeLogged()
        {
            List<string> outputs = new List<string>();
            _program.OnProcessOutput += output => outputs.Add(output);

            _ = await _program.StartAsync();
            _ = await _program.ConnectAsync();
            await _program.SendCommandAsync("test");

            Assert.That(outputs, Has.Count.GreaterThan(0));
            Assert.That(outputs, Has.Some.Contains("Process started"));
            Assert.That(outputs, Has.Some.Contains("Connected"));
            Assert.That(outputs, Has.Some.Contains("Command sent"));
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task Dispose_ShouldCleanupResources()
        {
            _ = await _program.StartAsync();
            _ = await _program.ConnectAsync();

            _program.Dispose();
            await Task.Delay(10);

            Assert.That(_program.IsRunning, Is.False);
            Assert.That(_program.IsConnected, Is.False);
        }

        [Test, Timeout(TestConstants.Timeouts.QUICK_TEST_TIMEOUT)]
        public async Task SimulatedDelays_ShouldRespectTimeouts()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            _ = await _program.StartAsync(); // 100ms
            _ = await _program.ConnectAsync(); // 50ms
            await _program.SendCommandAsync("test"); // 50ms
            _ = await _program.StopAsync(); // 100ms

            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;

            Assert.That(
                elapsedMs,
                Is.GreaterThanOrEqualTo(300),
                $"Expected at least 300ms but was {elapsedMs}ms"
            );
        }

        [TearDown]
        public void TearDown()
        {
            _program?.Dispose();
        }
    }
}

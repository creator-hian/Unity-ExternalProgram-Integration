using System;
using FAMOZ.ExternalProgram.Core;
using FAMOZ.ExternalProgram.Tests.Editor.Mocks;
using Mocks;
using NUnit.Framework;
using System.Collections.Generic;

namespace Process
{
    [TestFixture]
    public class ProcessTests
    {
        private MockExternalProgram _program;
        private MockLogger _logger;
        private MockCommunicationProtocol _protocol;
        private ProgramConfig _config;

        [SetUp]
        public void Setup()
        {
            _logger = new MockLogger();
            _protocol = new MockCommunicationProtocol();
            _config = new ProgramConfig(
                processName: "TestProcess",
                executablePath: "test.exe"
            );
            _program = new MockExternalProgram(
                _config,
                _protocol,
                _logger
            );
        }

        [Test]
        public void Start_ShouldSetIsRunning()
        {
            // Act
            bool result = _program.Start();

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_program.IsRunning, Is.True);
        }

        [Test]
        public void Stop_WhenRunning_ShouldSetIsRunningToFalse()
        {
            // Arrange
            _program.Start();

            // Act
            bool result = _program.Stop();

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_program.IsRunning, Is.False);
        }

        [Test]
        public void ProcessName_ShouldMatchConfig()
        {
            Assert.That(_program.ProcessName, Is.EqualTo(_config.ProcessName));
        }

        [Test]
        public void ExecutablePath_ShouldMatchConfig()
        {
            Assert.That(_program.ExecutablePath, Is.EqualTo(_config.ExecutablePath));
        }

        [Test]
        public void ProcessOutput_ShouldRaiseEvent()
        {
            // Arrange
            string receivedOutput = null;
            _program.OnProcessOutput += output => receivedOutput = output;
            _program.Start();

            // Act
            _program.EmitProcessOutput("test output");

            // Assert
            Assert.That(receivedOutput, Is.EqualTo("test output"));
        }

        [Test]
        public void ProcessError_ShouldRaiseEvent()
        {
            // Arrange
            string receivedError = null;
            _program.OnProcessError += error => receivedError = error;
            _program.Start();

            // Act
            _program.EmitProcessError("test error");

            // Assert
            Assert.That(receivedError, Is.EqualTo("test error"));
        }

        [Test]
        public void ProcessEvents_ShouldFollowCorrectOrder()
        {
            var outputs = new List<string>();
            var errors = new List<string>();
            var exitCodes = new List<int>();

            _program.OnProcessOutput += output => outputs.Add(output);
            _program.OnProcessError += error => errors.Add(error);
            _program.OnProcessExit += code => exitCodes.Add(code);

            // 시작
            _program.Start();
            Assert.That(outputs, Has.Some.Contains("Process started"));

            // 에러 발생
            _program.EmitProcessError("test error");
            Assert.That(errors, Has.Some.Contains("test error"));

            // 출력 발생
            _program.EmitProcessOutput("test output");
            Assert.That(outputs, Has.Some.Contains("test output"));

            // 종료
            _program.Stop();
            Assert.That(exitCodes, Has.Member(0));

            // 이벤트 순서 검증
            Assert.That(outputs.IndexOf("Process started") < outputs.IndexOf("test output"));
        }

        [Test]
        public void ProcessOutput_WithMultipleListeners_ShouldNotifyAll()
        {
            var outputs1 = new List<string>();
            var outputs2 = new List<string>();

            _program.OnProcessOutput += output => outputs1.Add(output);
            _program.OnProcessOutput += output => outputs2.Add(output);

            _program.Start();
            _program.EmitProcessOutput("test");

            Assert.That(outputs1, Is.EqualTo(outputs2));
            Assert.That(outputs1, Has.Count.EqualTo(2)); // "Process started" + "test"
        }

        [Test]
        public void Dispose_ShouldCleanupAllResources()
        {
            // 리소스 사용 설정
            _program.Start();
            Assert.That(_program.IsRunning, Is.True, "프로그램이 시작되어야 함");
            
            _program.Connect();
            Assert.That(_program.IsConnected, Is.True, "연결이 되어야 함");
            
            _program.EmitProcessOutput("test");

            // Dispose 호출
            _program.Dispose();

            // 상태 검증
            Assert.That(_program.IsRunning, Is.False, "실행 중이면 안됨");
            Assert.That(_program.IsConnected, Is.False, "연결되어 있으면 안됨");
            
            // Dispose 후 작업 시도 시 예외 발생 확인
            var ex1 = Assert.Throws<ObjectDisposedException>(() => _program.Start());
            Assert.That(ex1.ObjectName, Is.EqualTo(nameof(MockExternalProgram)));
            
            var ex2 = Assert.Throws<ObjectDisposedException>(() => _program.Connect());
            Assert.That(ex2.ObjectName, Is.EqualTo(nameof(MockExternalProgram)));
        }

        [Test]
        public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
        {
            _program.Start();

            Assert.DoesNotThrow(() =>
            {
                _program.Dispose();
                _program.Dispose(); // 두 번째 호출
            });
        }

        [TearDown]
        public void TearDown()
        {
            _program?.Dispose();
        }
    }
}
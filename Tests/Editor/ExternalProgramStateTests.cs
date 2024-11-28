using NUnit.Framework;
using System.Threading.Tasks;
using FAMOZ.ExternalProgram.Core;


/// <summary>
/// 외부 프로그램의 상태 관리 기능을 테스트합니다.
/// </summary>
public class ExternalProgramStateTests
    {
        private MockExternalProgram _program;
        private ProgramConfig _config;
        
        [SetUp]
        public void Setup()
        {
            _config = new ProgramConfig
            {
                ProcessName = "MockProcess",
                PortNumber = 12345
            };
            _program = new MockExternalProgram(_config);
        }
        
        /// <summary>
        /// 프로그램 시작 시 상태 전이를 검증합니다.
        /// 예상 결과:
        /// - 상태가 NotStarted에서 Running으로 변경
        /// - HasStarted가 true
        /// - IsConnected가 true
        /// </summary>
        [Test]
        public async Task StartAsync_ShouldChangeStateToRunning()
        {
            // Arrange
            Assert.That(_program.State, Is.EqualTo(ProgramState.NotStarted));
            
            // Act
            await _program.StartAsync();
            
            // Assert
            Assert.That(_program.State, Is.EqualTo(ProgramState.Running));
            Assert.That(_program.HasStarted, Is.True);
            Assert.That(_program.IsConnected, Is.True);
        }
        
        /// <summary>
        /// 프로그램 정지 시 상태 전이를 검증합니다.
        /// 예상 결과:
        /// - 상태가 Running에서 Stopped로 변경
        /// - HasCompleted가 true
        /// - IsConnected가 false
        /// </summary>
        [Test]
        public async Task StopAsync_ShouldChangeStateToStopped()
        {
            // Arrange
            await _program.StartAsync();
            
            // Act
            await _program.StopAsync();
            
            // Assert
            Assert.That(_program.State, Is.EqualTo(ProgramState.Stopped));
            Assert.That(_program.HasCompleted, Is.True);
            Assert.That(_program.IsConnected, Is.False);
        }
    }

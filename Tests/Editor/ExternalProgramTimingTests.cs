using NUnit.Framework;
using System.Threading.Tasks;
using FAMOZ.ExternalProgram.Core;

/// <summary>
/// 외부 프로그램의 시간 관련 기능을 테스트합니다.
/// </summary>
public class ExternalProgramTimingTests
    {
        /// <summary>
        /// 프로그램 실행 시간 계산의 정확성을 검증합니다.
        /// 예상 결과:
        /// - 1초 지연 후 RunningTime이 1초 이상
        /// - 지연 오차를 고려하여 2초 미만
        /// </summary>
        [Test]
        public async Task RunningTime_ShouldCalculateCorrectly()
        {
            // Arrange
            var program = new MockExternalProgram(new ProgramConfig());
            
            // Act
            await program.StartAsync();
            await Task.Delay(1000);
            await program.StopAsync();
            
            // Assert
            Assert.That(program.RunningTime.TotalSeconds, Is.GreaterThanOrEqualTo(1));
            Assert.That(program.RunningTime.TotalSeconds, Is.LessThan(2));
        }
    }
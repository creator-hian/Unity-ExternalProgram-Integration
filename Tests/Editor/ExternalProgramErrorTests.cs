using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using FAMOZ.ExternalProgram.Core;
using UnityEngine;

/// <summary>
/// 외부 프로그램의 에러 처리 기능을 테스트합니다.
/// </summary>
public class ExternalProgramErrorTests
{
    private MockExternalProgram _program;
    private List<ProgramError> _errors;

    [SetUp]
    public void Setup()
    {
        _errors = new List<ProgramError>();
        _program = new MockExternalProgram(new ProgramConfig());
        _program.OnError += error => _errors.Add(error);
    }

    /// <summary>
    /// 연결되지 않은 상태에서 명령 전송 시 에러 발생을 검증합니다.
    /// 예상 결과:
    /// - InvalidOperationException 발생
    /// - ErrorType.InvalidOperation 타입의 에러 이벤트 발생
    /// - 에러 로그 메시지 출력
    /// </summary>
    [Test]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task SendCommand_WhenNotConnected_ShouldRaiseError()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        // Arrange
        LogAssert.Expect(LogType.Error, "ExternalProgram Error: Cannot send command: Not connected");
        // Act
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _program.SendCommandAsync("test")
        );

        // Assert
        Assert.That(_errors, Has.Count.EqualTo(1));
        Assert.That(_errors[0].Type, Is.EqualTo(ErrorType.InvalidOperation));
    }
}

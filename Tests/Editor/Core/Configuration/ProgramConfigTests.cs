using System;
using Hian.ExternalProgram.Core;
using NUnit.Framework;

namespace Configuration
{
    public class ProgramConfigTests
    {
        private ProgramConfig _config;

        [SetUp]
        public void Setup()
        {
            _config = new ProgramConfig(
                processName: "TestProcess",
                executablePath: "C:/test/program.exe",
                arguments: "-test",
                portNumber: 12345
            );
        }

        [Test]
        public void Constructor_WithValidValues_ShouldInitializeCorrectly()
        {
            ProgramConfig config = new ProgramConfig(
                processName: "TestProcess",
                executablePath: "test.exe",
                protocolType: "TCP",
                portNumber: 12345,
                arguments: "-test"
            );

            Assert.That(config.ProcessName, Is.EqualTo("TestProcess"));
            Assert.That(config.ExecutablePath, Is.EqualTo("test.exe"));
            Assert.That(config.ProtocolType, Is.EqualTo("TCP"));
            Assert.That(config.PortNumber, Is.EqualTo(12345));
            Assert.That(config.Arguments, Is.EqualTo("-test"));
        }

        [Test]
        public void Constructor_WithEmptyProcessName_ShouldThrow()
        {
            _ = Assert.Throws<ArgumentException>(
                static () => new ProgramConfig(processName: "", executablePath: "test.exe")
            );
        }

        [Test]
        public void Constructor_WithEmptyExecutablePath_ShouldThrow()
        {
            _ = Assert.Throws<ArgumentException>(
                static () => new ProgramConfig(processName: "TestProcess", executablePath: "")
            );
        }

        [Test]
        public void Constructor_WithEmptyProtocolType_ShouldThrow()
        {
            _ = Assert.Throws<ArgumentException>(
                static () =>
                    new ProgramConfig(
                        processName: "TestProcess",
                        executablePath: "test.exe",
                        protocolType: ""
                    )
            );
        }

        [Test]
        public void Clone_ShouldCreateDeepCopy()
        {
            // Act
            ProgramConfig clone = _config.Clone();
            ProgramConfig newConfig = new ProgramConfig(
                processName: "TestProcess",
                executablePath: _config.ExecutablePath,
                arguments: _config.Arguments,
                portNumber: 54321
            );
            _config = newConfig;

            // Assert
            Assert.That(clone.PortNumber, Is.EqualTo(12345));
            Assert.That(clone.ProcessName, Is.EqualTo(_config.ProcessName));
        }

        [Test]
        public void ToJson_ShouldSerializeAllProperties()
        {
            // Act
            string json = _config.ToJson();

            // Assert - 각 필드가 JSON에 포함되어 있는지 확인
            Assert.That(json, Does.Contain("\"processName\""));
            Assert.That(json, Does.Contain("\"executablePath\""));
            Assert.That(json, Does.Contain("\"protocolType\""));
            Assert.That(json, Does.Contain("\"portNumber\""));
            Assert.That(json, Does.Contain("\"arguments\""));

            // 값 검증
            Assert.That(json, Does.Contain(_config.ProcessName));
            Assert.That(json, Does.Contain(_config.ExecutablePath));
            Assert.That(json, Does.Contain(_config.Arguments));
        }

        [Test]
        public void FromJson_WithValidJson_ShouldDeserializeCorrectly()
        {
            // Arrange
            string json = _config.ToJson();

            // Act
            ProgramConfig deserializedConfig = ProgramConfig.FromJson(json);

            // Assert
            Assert.That(deserializedConfig.ProcessName, Is.EqualTo(_config.ProcessName));
            Assert.That(deserializedConfig.ExecutablePath, Is.EqualTo(_config.ExecutablePath));
            Assert.That(deserializedConfig.ProtocolType, Is.EqualTo(_config.ProtocolType));
            Assert.That(deserializedConfig.PortNumber, Is.EqualTo(_config.PortNumber));
            Assert.That(deserializedConfig.Arguments, Is.EqualTo(_config.Arguments));
            Assert.That(deserializedConfig.StartTimeoutMs, Is.EqualTo(_config.StartTimeoutMs));
            Assert.That(deserializedConfig.StopTimeoutMs, Is.EqualTo(_config.StopTimeoutMs));
            Assert.That(deserializedConfig.MaxRetryAttempts, Is.EqualTo(_config.MaxRetryAttempts));
        }

        [Test]
        public void FromJson_WithInvalidJson_ShouldThrow()
        {
            _ = Assert.Throws<ArgumentException>(
                static () => ProgramConfig.FromJson("invalid json")
            );
        }

        [Test]
        public void FromJson_WithEmptyJson_ShouldThrow()
        {
            _ = Assert.Throws<ArgumentException>(static () => ProgramConfig.FromJson(""));
        }
    }
}

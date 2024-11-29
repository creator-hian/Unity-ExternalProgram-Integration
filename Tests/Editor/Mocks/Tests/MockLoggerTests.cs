using NUnit.Framework;
using FAMOZ.ExternalProgram.Tests.Editor.Mocks;
using Mocks;

namespace Mocks
{
    public class MockLoggerTests
    {
        private MockLogger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = new MockLogger();
        }

        [Test]
        public void Logger_ShouldCaptureAllLogTypes()
        {
            _logger.Log("info message");
            _logger.LogWarning("warning message");
            _logger.LogError("error message");
            
            Assert.That(_logger.InfoLogs, Has.Count.EqualTo(1));
            Assert.That(_logger.WarningLogs, Has.Count.EqualTo(1));
            Assert.That(_logger.ErrorLogs, Has.Count.EqualTo(1));
            
            Assert.That(_logger.InfoLogs[0], Is.EqualTo("info message"));
            Assert.That(_logger.WarningLogs[0], Is.EqualTo("warning message"));
            Assert.That(_logger.ErrorLogs[0], Is.EqualTo("error message"));
        }

        [Test]
        public void Logger_ShouldMaintainLogOrder()
        {
            _logger.Log("1");
            _logger.Log("2");
            _logger.Log("3");
            
            Assert.That(_logger.InfoLogs, Is.EqualTo(new[] { "1", "2", "3" }));
        }

        [Test]
        public void Clear_ShouldRemoveAllLogs()
        {
            _logger.Log("test");
            _logger.LogWarning("test");
            _logger.LogError("test");
            
            _logger.Clear();
            
            Assert.That(_logger.InfoLogs, Is.Empty);
            Assert.That(_logger.WarningLogs, Is.Empty);
            Assert.That(_logger.ErrorLogs, Is.Empty);
        }
    }
} 
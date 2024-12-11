using NUnit.Framework;
using System.Threading.Tasks;
using Hian.ExternalProgram.Tests.Editor.Mocks;

namespace Communication
{
    [TestFixture]
    [Timeout(5000)]
    public class CommunicationProtocolTests
    {
        private MockCommunicationProtocol _protocol;

        [SetUp]
        public void Setup()
        {
            _protocol = new MockCommunicationProtocol();
        }

        [Test, Timeout(2000)]
        public async Task Connect_ShouldChangeConnectionState()
        {
            Assert.That(_protocol.IsConnected, Is.False);
            
            bool result = await _protocol.ConnectAsync();
            
            Assert.That(result, Is.True);
            Assert.That(_protocol.IsConnected, Is.True);
        }

        [Test, Timeout(2000)]
        public async Task SendData_WhenConnected_ShouldRaiseEvent()
        {
            byte[] receivedData = null;
            _protocol.OnDataReceived += data => receivedData = data;

            await _protocol.ConnectAsync();
            byte[] testData = new byte[] { 1, 2, 3 };
            await _protocol.SendAsync(testData);

            Assert.That(receivedData, Is.EqualTo(testData));
        }

        [TearDown]
        public void TearDown()
        {
            _protocol?.Dispose();
        }
    }
} 
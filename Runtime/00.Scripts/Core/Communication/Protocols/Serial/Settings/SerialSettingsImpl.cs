using System.IO.Ports;
using Hian.ExternalProgram.Core.Communication.Settings;

namespace Hian.ExternalProgram.Core.Communication.Protocols.Serial
{
    public class SerialSettingsImpl : ProtocolSettingsBase, ISerialSettings
    {
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Parity Parity { get; set; } = Parity.None;
        public Handshake Handshake { get; set; } = Handshake.None;
        public bool DtrEnable { get; set; } = false;
        public bool RtsEnable { get; set; } = false;
        public bool DiscardNull { get; set; } = false;
        public byte ParityReplace { get; set; } = (byte)'?';
        public bool BreakState { get; set; } = false;
        public bool TxContinueOnXoff { get; set; } = false;
        public byte XoffChar { get; set; } = 0x13;
        public byte XonChar { get; set; } = 0x11;

        public override object Clone()
        {
            return new SerialSettingsImpl
            {
                BufferSize = BufferSize,
                MaxMessageSize = MaxMessageSize,
                ConnectionTimeoutMs = ConnectionTimeoutMs,
                SendTimeoutMs = SendTimeoutMs,
                ReceiveTimeoutMs = ReceiveTimeoutMs,
                EnableReconnect = EnableReconnect,
                MaxReconnectAttempts = MaxReconnectAttempts,
                ReconnectDelayMs = ReconnectDelayMs,
                MaxReconnectDelayMs = MaxReconnectDelayMs,
                EnableCompression = EnableCompression,
                EnableEncryption = EnableEncryption,
                MaxConcurrentOperations = MaxConcurrentOperations,
                EnableDebugLogging = EnableDebugLogging,
                EnableMetrics = EnableMetrics,
                MaxMetricsCount = MaxMetricsCount,
                BaudRate = BaudRate,
                DataBits = DataBits,
                StopBits = StopBits,
                Parity = Parity,
                Handshake = Handshake,
                DtrEnable = DtrEnable,
                RtsEnable = RtsEnable,
                DiscardNull = DiscardNull,
                ParityReplace = ParityReplace,
                BreakState = BreakState,
                TxContinueOnXoff = TxContinueOnXoff,
                XoffChar = XoffChar,
                XonChar = XonChar,
            };
        }

        public override bool Validate()
        {
            if (!base.Validate())
            {
                return false;
            }

            if (BaudRate <= 0)
            {
                return false;
            }

            return DataBits is >= 5 and <= 8;
        }
    }
}

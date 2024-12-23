namespace Hian.ExternalProgram.Core.Communication.Protocols
{
    public class TcpProtocolProvider : ICommunicationProtocolProvider
    {
        public string ProtocolType => "TCP";

        public ICommunicationProtocol CreateProtocol(ProgramConfig config)
        {
            return new TcpProtocol(ExternalProgramConstants.LOCAL_ADDRESS, config.PortNumber);
        }
    }
}

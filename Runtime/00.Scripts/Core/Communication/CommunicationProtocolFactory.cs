using System;
using System.Collections.Generic;

namespace FAMOZ.ExternalProgram.Core.Communication
{
    /// <summary>
    /// 통신 프로토콜 생성을 담당하는 팩토리 클래스입니다.
    /// 새로운 프로토콜 제공자를 등록하고 관리합니다.
    /// </summary>
    public class CommunicationProtocolFactory
    {
        /// <summary>
        /// 등록된 프로토콜 제공자 목록
        /// </summary>
        private readonly Dictionary<string, ICommunicationProtocolProvider> _providers 
            = new Dictionary<string, ICommunicationProtocolProvider>();

        /// <summary>
        /// 새로운 프로토콜 제공자를 등록합니다.
        /// </summary>
        /// <param name="provider">등록할 프로토콜 제공자</param>
        /// <exception cref="ArgumentNullException">provider가 null인 경우</exception>
        public void RegisterProvider(ICommunicationProtocolProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _providers[provider.ProtocolType] = provider;
        }

        /// <summary>
        /// 주어진 설정에 맞는 통신 프로토콜을 생성합니다.
        /// </summary>
        /// <param name="config">프로그램 설정</param>
        /// <returns>생성된 통신 프로토콜 인스턴스</returns>
        /// <exception cref="InvalidOperationException">해당하는 프로토콜 제공자가 없는 경우</exception>
        public ICommunicationProtocol Create(ProgramConfig config)
        {
            if (string.IsNullOrEmpty(config.ProtocolType))
                return null;

            if (_providers.TryGetValue(config.ProtocolType, out var provider))
            {
                return provider.CreateProtocol(config);
            }

            throw new InvalidOperationException($"No provider registered for protocol type: {config.ProtocolType}");
        }
    }
} 
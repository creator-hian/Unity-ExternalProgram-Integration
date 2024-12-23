using System;
using Hian.ExternalProgram.Core.Communication.Settings;

namespace Hian.ExternalProgram.Core.Communication.Protocols.InMemory
{
    /// <summary>
    /// 인메모리 통신 프로토콜의 설정을 정의하는 인터페이스입니다.
    /// </summary>
    public interface IInMemorySettings : IProtocolSettings
    {
        // 메시지 처리 설정
        /// <summary>
        /// 메시지 우선순위 기능의 활성화 여부를 가져오니다.
        /// </summary>
        bool EnablePriority { get; }

        /// <summary>
        /// 메시지 배치 처리 기능의 활성화 여부를 가져옵니다.
        /// </summary>
        bool EnableBatching { get; }

        /// <summary>
        /// 메시지 만료 기능의 활성화 여부를 가져옵니다.
        /// </summary>
        bool EnableMessageExpiry { get; }

        /// <summary>
        /// 배치 처리할 메시지의 최대 크기를 가져옵니다.
        /// </summary>
        int BatchSize { get; }

        /// <summary>
        /// 배치 처리의 타임아웃 시간(밀리초)을 가져옵니다.
        /// </summary>
        int BatchTimeout { get; }

        /// <summary>
        /// 메시지의 생존 시간(TTL)을 가져옵니다.
        /// </summary>
        TimeSpan MessageTtl { get; }

        // 큐 설정
        /// <summary>
        /// 큐의 최대 용량을 가져옵니다.
        /// </summary>
        int QueueCapacity { get; }

        /// <summary>
        /// 메시지 순서 보장 기능의 활성화 여부를 가져옵니다.
        /// </summary>
        bool EnableOrdering { get; }

        /// <summary>
        /// 최대 우선순위 레벨을 가져옵니다.
        /// </summary>
        int MaxPriorityLevels { get; }

        // 메시지 관리
        /// <summary>
        /// 메시지 보관 기능의 활성화 여부를 가져옵니다.
        /// </summary>
        bool RetainMessages { get; }

        /// <summary>
        /// 최대 보관 메시지 수를 가져옵니다.
        /// </summary>
        int MaxRetainedMessages { get; }
    }
}

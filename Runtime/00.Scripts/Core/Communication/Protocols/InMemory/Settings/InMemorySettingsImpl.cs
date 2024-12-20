using System;
using Hian.ExternalProgram.Core.Communication.Settings;

namespace Hian.ExternalProgram.Core.Communication.Protocols.InMemory
{
    public class InMemorySettingsImpl : ProtocolSettingsBase, IInMemorySettings
    {
        public int QueueCapacity { get; set; } = 1000;
        public bool EnableOrdering { get; set; } = true;
        public bool EnablePriority { get; set; } = false;
        public int MaxPriorityLevels { get; set; } = 3;
        public TimeSpan MessageTtl { get; set; } = TimeSpan.FromMinutes(1);
        public bool EnableMessageExpiry { get; set; } = true;
        public bool RetainMessages { get; set; } = false;
        public int MaxRetainedMessages { get; set; } = 1000;
        public bool EnableBatching { get; set; } = false;
        public int BatchSize { get; set; } = 100;
        public int BatchTimeout { get; set; } = 100;

        public override object Clone()
        {
            return new InMemorySettingsImpl
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
                QueueCapacity = QueueCapacity,
                EnableOrdering = EnableOrdering,
                EnablePriority = EnablePriority,
                MaxPriorityLevels = MaxPriorityLevels,
                MessageTtl = MessageTtl,
                EnableMessageExpiry = EnableMessageExpiry,
                RetainMessages = RetainMessages,
                MaxRetainedMessages = MaxRetainedMessages,
                EnableBatching = EnableBatching,
                BatchSize = BatchSize,
                BatchTimeout = BatchTimeout,
            };
        }

        public override bool Validate()
        {
            if (!base.Validate())
                return false;
            if (QueueCapacity <= 0)
                return false;
            if (MaxPriorityLevels <= 0)
                return false;
            if (MessageTtl <= TimeSpan.Zero)
                return false;
            if (MaxRetainedMessages < 0)
                return false;
            if (BatchSize <= 0)
                return false;
            if (BatchTimeout <= 0)
                return false;
            return true;
        }
    }
}

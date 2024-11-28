using System;

namespace FAMOZ.ExternalProgram.Core
{
    [Serializable]
    public class ProgramConfig
    {
        public string ProcessName { get; set; }
        public int PortNumber { get; set; }
        public string ProgramPath { get; set; }
        public TimeSpan ProcessTimeout { get; set; }
        public int MaxRetryAttempts { get; set; }
        public TimeSpan RetryDelay { get; set; }
        
        public ProgramConfig()
        {
            ProcessTimeout = TimeSpan.FromMilliseconds(ExternalProgramConstants.Timeouts.DEFAULT_START_TIMEOUT_MS);
            MaxRetryAttempts = ExternalProgramConstants.RetryPolicy.DEFAULT_MAX_RETRY_ATTEMPTS;
            RetryDelay = TimeSpan.FromMilliseconds(ExternalProgramConstants.RetryPolicy.DEFAULT_RETRY_DELAY_MS);
        }
    }
} 
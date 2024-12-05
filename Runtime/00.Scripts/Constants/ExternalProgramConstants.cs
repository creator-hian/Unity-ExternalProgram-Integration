namespace Hian.ExternalProgram
{
    public static class ExternalProgramConstants
    {
        public const string PORT_ARGUMENT_STRING = "port";
        public const char COMMANDLINE_DELIMITER = '-';
        public const string SPACE_STRING = " ";
        public const string LOCAL_ADDRESS = "127.0.0.1";
        
        public static class Timeouts
        {
            public const int DEFAULT_START_TIMEOUT_MS = 5000;
            public const int DEFAULT_STOP_TIMEOUT_MS = 3000;
            public const int DEFAULT_COMMAND_TIMEOUT_MS = 1000;
        }
        
        public static class RetryPolicy
        {
            public const int DEFAULT_MAX_RETRY_ATTEMPTS = 3;
            public const int DEFAULT_RETRY_DELAY_MS = 1000;
        }
    }
} 
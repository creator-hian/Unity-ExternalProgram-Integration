namespace Hian.ExternalProgram.Tests.Editor.Core
{
    public static class TestConstants
    {
        public static class Timeouts
        {
            public const int DEFAULT_TEST_TIMEOUT = 5000; // 5초
            public const int QUICK_TEST_TIMEOUT = 2000; // 2초
            public const int LONG_TEST_TIMEOUT = 10000; // 10초
        }

        public static class Delays
        {
            public const int PROCESS_START_DELAY = 100; // 100ms
            public const int PROCESS_STOP_DELAY = 100; // 100ms
            public const int COMMUNICATION_DELAY = 50; // 50ms
        }
    }
}

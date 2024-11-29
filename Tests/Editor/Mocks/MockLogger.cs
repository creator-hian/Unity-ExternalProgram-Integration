using System.Collections.Generic;
using FAMOZ.ExternalProgram.Core;

namespace Mocks
{
    public class MockLogger : ILogger
    {
        public List<string> InfoLogs { get; } = new List<string>();
        public List<string> WarningLogs { get; } = new List<string>();
        public List<string> ErrorLogs { get; } = new List<string>();
        public List<System.Exception> Exceptions { get; } = new List<System.Exception>();

        public void Log(string message) => InfoLogs.Add(message);
        public void LogWarning(string message) => WarningLogs.Add(message);
        public void LogError(string message) => ErrorLogs.Add(message);
        public void LogException(System.Exception ex) => Exceptions.Add(ex);

        public void Clear()
        {
            InfoLogs.Clear();
            WarningLogs.Clear();
            ErrorLogs.Clear();
            Exceptions.Clear();
        }
    }
} 
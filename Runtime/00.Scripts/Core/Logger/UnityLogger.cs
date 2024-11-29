using UnityEngine;

namespace FAMOZ.ExternalProgram.Core
{
    public class UnityLogger : ILogger
    {
        public void Log(string message) => Debug.Log($"[ExternalProgram] {message}");
        public void LogWarning(string message) => Debug.LogWarning($"[ExternalProgram] {message}");
        public void LogError(string message) => Debug.LogError($"[ExternalProgram] {message}");
        public void LogException(System.Exception exception) => Debug.LogException(exception);
    }
} 
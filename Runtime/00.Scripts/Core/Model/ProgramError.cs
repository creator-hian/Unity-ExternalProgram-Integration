using System;

namespace Hian.ExternalProgram.Core
{
    public class ProgramError
    {
        public string Message { get; }
        public string StackTrace { get; }
        public ErrorType Type { get; }
        public Exception Exception { get; }

        public ProgramError(string message, ErrorType type, Exception exception = null)
        {
            Message = message;
            Type = type;
            Exception = exception;
            StackTrace = exception?.StackTrace;
        }
    }
}

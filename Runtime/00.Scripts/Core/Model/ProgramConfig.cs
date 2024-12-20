using System;
using UnityEngine;

namespace Hian.ExternalProgram.Core
{
    /// <summary>
    /// 외부 프로그램 실행 및 통신에 필요한 설정을 정의하는 클래스입니다.
    /// </summary>
    public class ProgramConfig
    {
        [SerializeField]
        private string processName;

        [SerializeField]
        private string executablePath;

        [SerializeField]
        private string arguments;

        [SerializeField]
        private string protocolType;

        [SerializeField]
        private int portNumber;

        [SerializeField]
        private int startTimeoutMs;

        [SerializeField]
        private int stopTimeoutMs;

        [SerializeField]
        private int maxRetryAttempts;

        // 프로퍼티는 필드를 반환하도록 수정
        public string ProcessName => processName;
        public string ExecutablePath => executablePath;
        public string Arguments => arguments;
        public string ProtocolType => protocolType;
        public int PortNumber => portNumber;
        public int StartTimeoutMs => startTimeoutMs;
        public int StopTimeoutMs => stopTimeoutMs;
        public int MaxRetryAttempts => maxRetryAttempts;

        // 기본 생성자 추가 (JsonUtility용)
        public ProgramConfig() { }

        public ProgramConfig(
            string processName,
            string executablePath,
            string protocolType = "TCP",
            int portNumber = 0,
            string arguments = "",
            int startTimeoutMs = ExternalProgramConstants.Timeouts.DEFAULT_START_TIMEOUT_MS,
            int stopTimeoutMs = ExternalProgramConstants.Timeouts.DEFAULT_STOP_TIMEOUT_MS,
            int maxRetryAttempts = ExternalProgramConstants.RetryPolicy.DEFAULT_MAX_RETRY_ATTEMPTS
        )
        {
            if (string.IsNullOrEmpty(processName))
            {
                throw new ArgumentException("Process name cannot be empty", nameof(processName));
            }

            if (string.IsNullOrEmpty(executablePath))
            {
                throw new ArgumentException(
                    "Executable path cannot be empty",
                    nameof(executablePath)
                );
            }

            if (string.IsNullOrEmpty(protocolType))
            {
                throw new ArgumentException("Protocol type cannot be empty", nameof(protocolType));
            }

            this.processName = processName;
            this.executablePath = executablePath;
            this.protocolType = protocolType;
            this.portNumber = portNumber;
            this.arguments = arguments ?? "";
            this.startTimeoutMs = startTimeoutMs;
            this.stopTimeoutMs = stopTimeoutMs;
            this.maxRetryAttempts = maxRetryAttempts;
        }

        /// <summary>
        /// 현재 설정의 복사본을 생성합니다.
        /// </summary>
        /// <returns>설정 복사본</returns>
        public ProgramConfig Clone()
        {
            return new ProgramConfig(
                ProcessName,
                ExecutablePath,
                ProtocolType,
                PortNumber,
                Arguments,
                StartTimeoutMs,
                StopTimeoutMs,
                MaxRetryAttempts
            );
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public static ProgramConfig FromJson(string json)
        {
            return JsonUtility.FromJson<ProgramConfig>(json);
        }
    }
}

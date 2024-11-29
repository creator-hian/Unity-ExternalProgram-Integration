using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;

namespace FAMOZ.ExternalProgram.Core.Manager
{
    /// <summary>
    /// 외부 프로그램들을 관리하는 싱글톤 매니저 클래스입니다.
    /// </summary>
    public class ExternalProgramManager : IDisposable
    {
        private static readonly Lazy<ExternalProgramManager> _instance = 
            new Lazy<ExternalProgramManager>(() => new ExternalProgramManager());

        /// <summary>
        /// 싱글톤 인스턴스를 가져옵니다.
        /// </summary>
        public static ExternalProgramManager Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, IExternalProgram> _programs;
        private ILogger _logger;

        // 외부에서 인스턴스화 방지
        private ExternalProgramManager()
        {
            _programs = new ConcurrentDictionary<string, IExternalProgram>();
            _logger = new UnityLogger();
        }

        /// <summary>
        /// 로거를 설정합니다.
        /// </summary>
        public void SetLogger(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            _logger = logger;
        }

        /// <summary>
        /// 프로그램을 등록합니다.
        /// </summary>
        public bool RegisterProgram(IExternalProgram program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            bool added = _programs.TryAdd(program.ProcessName, program);
            if (added)
                _logger.Log($"Program registered: {program.ProcessName}");
            return added;
        }

        /// <summary>
        /// 프로그램을 제거합니다.
        /// </summary>
        public async Task<bool> UnregisterProgramAsync(string programName)
        {
            if (_programs.TryGetValue(programName, out var program))
            {
                if (program.IsRunning)
                    await program.StopAsync();

                bool removed = _programs.TryRemove(programName, out _);
                if (removed)
                    _logger.Log($"Program unregistered: {programName}");
                return removed;
            }
            return false;
        }

        /// <summary>
        /// 등록된 프로그램을 가져옵니다.
        /// </summary>
        public IExternalProgram GetProgram(string programName)
        {
            _programs.TryGetValue(programName, out var program);
            return program;
        }

        /// <summary>
        /// 모든 프로그램을 정리합니다.
        /// </summary>
        public async Task CleanupAsync()
        {
            var stopTasks = _programs.Values.Select(p => p.StopAsync());
            await Task.WhenAll(stopTasks);
            
            foreach (var program in _programs.Values)
            {
                program.Dispose();
            }
            _programs.Clear();
        }

        public void Dispose()
        {
            CleanupAsync().Wait();
        }
    }
}
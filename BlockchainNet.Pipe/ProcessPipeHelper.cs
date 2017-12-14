namespace BlockchainNet.Pipe
{
    using System.Linq;
    using System.Diagnostics;
    using System.Collections.Generic;

    public static class ProcessPipeHelper
    {
        /// <summary>
        /// Возвращает Id канала для текущего процесса
        /// </summary>
        /// <returns>Id канала</returns>
        public static string GetCurrentPipeId()
        {
            var currentProcess = Process.GetCurrentProcess();
            return GetPipeIdFromProcessId(currentProcess.Id);
        }

        /// <summary>
        /// Возвращает Id каналов с соседних процесов
        /// </summary>
        /// <returns>Id каналов соседних процессов</returns>
        public static IEnumerable<string> GetNeighborPipesIds()
        {
            // важно учесть .vshost для процесса с дебагером

            var currentProcess = Process.GetCurrentProcess();
            var processName = currentProcess.ProcessName.Replace(".vshost", "");
            var processNameVsHosted = currentProcess.ProcessName + ".vshost";

            return Process
                .GetProcessesByName(processName)
                .Concat(Process.GetProcessesByName(processNameVsHosted))
                .Where(p => p.Id != currentProcess.Id)
                .Select(p => GetPipeIdFromProcessId(p.Id));
        }

        private static string GetPipeIdFromProcessId(int processId)
        {
            return $"Pipe-{processId}";
        }
    }
}

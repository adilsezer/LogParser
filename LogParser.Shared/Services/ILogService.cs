using LogParser.Shared.Models;

namespace LogParser.Shared.Services
{
    public interface ILogService
    {
        Task<IEnumerable<LogEntry>> LoadLogsAsync(IEnumerable<string> filePaths);
        IEnumerable<LogEntry> QueryLogs(IEnumerable<LogEntry> logs, string query);
        string GetLogsAsJson(IEnumerable<LogEntry> logs, string query);
    }
}

using LogParser.Shared.Models;
using LogParser.Shared.Utilities;
using System.Text.Json;

namespace LogParser.Shared.Services
{
    public class LogService
    {
        private readonly CsvParser _csvHelper;
        private readonly IQueryParser _queryParser;

        public LogService(CsvParser csvHelper, IQueryParser queryParser)
        {
            _csvHelper = csvHelper ?? throw new ArgumentNullException(nameof(csvHelper));
            _queryParser = queryParser ?? throw new ArgumentNullException(nameof(queryParser));
        }

        public async Task<IEnumerable<LogEntry>> LoadLogsAsync(IEnumerable<string> filePaths)
        {
            if (filePaths == null || !filePaths.Any())
                throw new ArgumentException("No file paths provided.", nameof(filePaths));

            var logs = new List<LogEntry>();
            foreach (var filePath in filePaths)
                logs.AddRange(await _csvHelper.ReadLogsAsync(filePath));

            return logs;
        }

        public IEnumerable<LogEntry> QueryLogs(IEnumerable<LogEntry> logs, string query)
        {
            if (logs == null)
                throw new ArgumentNullException(nameof(logs));

            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            var predicate = _queryParser.Parse(query);
            return logs.Where(predicate);
        }

        public string GetLogsAsJson(IEnumerable<LogEntry> logs, string query)
        {
            if (logs == null)
                throw new ArgumentNullException(nameof(logs));

            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            var result = new
            {
                searchQuery = query,
                logCount = logs.Count(),
                results = logs
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

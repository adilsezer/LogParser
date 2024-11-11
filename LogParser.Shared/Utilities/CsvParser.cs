using CsvHelper;
using CsvHelper.Configuration;
using LogParser.Shared.Models;
using System.Globalization;

namespace LogParser.Shared.Utilities
{
    public class CsvParser
    {
        public async Task<IEnumerable<LogEntry>> ReadLogsAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                MissingFieldFound = null,
                HeaderValidated = null,
                HasHeaderRecord = true
            };

            using var csv = new CsvReader(reader, csvConfig);

            var records = new List<LogEntry>();
            await foreach (var record in csv.GetRecordsAsync<LogEntry>())
            {
                records.Add(record);
            }

            return records;
        }
    }
}
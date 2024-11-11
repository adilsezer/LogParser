using LogParser.Shared.Configuration;
using LogParser.Shared.Services;
using LogParser.Shared.Utilities;
using System.Text.Json;

namespace LogParser.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to LogParser!");

            var config = LoadConfiguration("appsettings.json");
            var logReader = new CsvParser();
            var queryParser = new QueryParser();
            var logService = new LogService(logReader, queryParser);

            var filePaths = GetFilePathsFromUser(config.LogParserSettings.DefaultFilePath);

            if (filePaths.Count == 0)
            {
                Console.WriteLine("No valid file paths provided. Exiting...");
                return;
            }

            var logs = await logService.LoadLogsAsync(filePaths);

            while (true)
            {
                Console.WriteLine("Enter your query (e.g., severity=5 AND signatureId='*4608*') or type 'exit' to quit:");
                var query = Console.ReadLine();

                if (string.Equals(query, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Exiting LogParser. Goodbye!");
                    break;
                }

                if (string.IsNullOrWhiteSpace(query))
                {
                    Console.WriteLine("No query provided. Please try again.");
                    continue;
                }

                try
                {
                    var filteredLogs = logService.QueryLogs(logs, query);

                    Console.WriteLine("Filtered Logs:");
                    Console.WriteLine(logService.GetLogsAsJson(filteredLogs, query));

                    foreach (var log in filteredLogs)
                    {
                        if (log.Severity >= config.LogParserSettings.SeverityThreshold)
                        {
                            Console.WriteLine($"ALERT: Log with Severity {log.Severity} - {log.Msg}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        static AppSettings LoadConfiguration(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found: {filePath}");

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var settings = JsonSerializer.Deserialize<AppSettings>(json, options);

            if (settings == null)
                throw new InvalidOperationException("Failed to load configuration. The configuration file might be invalid.");

            return settings;
        }

        static List<string> GetFilePathsFromUser(string defaultFilePath)
        {
            Console.WriteLine("Enter the paths to CSV files, separated by commas. Press Enter to use the default file:");
            var input = Console.ReadLine();
            var filePaths = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
            {
                if (File.Exists(defaultFilePath))
                    filePaths.Add(defaultFilePath);
                else
                    Console.WriteLine($"Default file not found: {defaultFilePath}");
            }
            else
            {
                var paths = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var path in paths)
                {
                    var trimmedPath = path.Trim();
                    if (File.Exists(trimmedPath))
                        filePaths.Add(trimmedPath);
                    else
                        Console.WriteLine($"File not found: {trimmedPath}");
                }
            }

            return filePaths;
        }
    }
}

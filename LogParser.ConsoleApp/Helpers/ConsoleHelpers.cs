using LogParser.ConsoleApp.Configuration;
using LogParser.Utilities.Models;
using LogParser.Utilities.Services;

namespace LogParser.ConsoleApp.Helpers
{
    public static class ConsoleHelpers
    {
        public static AppSettings LoadConfiguration(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found");
            }

            var json = File.ReadAllText(filePath);
            var settings = JsonUtility.Deserialize<AppSettings>(json);

            return settings ?? throw new InvalidOperationException("Failed to load the configuration");
        }

        public static List<string> GetFilePathsFromUser(string defaultFilePath)
        {
            Console.WriteLine("Enter the paths of CSV files, separate them by commas. Press Enter to use the default file:");
            var input = Console.ReadLine();
            var filePaths = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
            {
                if (File.Exists(defaultFilePath))
                {
                    filePaths.Add(defaultFilePath);
                }
                else
                {
                    Console.WriteLine($"Default file not found");
                }
            }
            else
            {
                var paths = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var path in paths)
                {
                    if (File.Exists(path.Trim()))
                    {
                        filePaths.Add(path.Trim());
                    }
                    else
                    {
                        Console.WriteLine($"This file not found: {path.Trim()}");
                    }
                }
            }

            return filePaths;
        }

        public static int GetSeverityThreshold()
        {
            Console.WriteLine("Please enter the severity threshold as a number: ");
            if (!int.TryParse(Console.ReadLine(), out int severityThreshold))
            {
                Console.WriteLine("Invalid input. We will use severity threshold as 3.");
                severityThreshold = 3;
            }

            return severityThreshold;
        }

        public static void DisplayResults(QueryResult results, string query, int severityThreshold)
        {
            var alerts = new List<string>();

            foreach (var log in results.Logs)
            {
                if (log.Fields.TryGetValue("severity", out var severityObj) &&
                    int.TryParse(severityObj?.ToString(), out int severity) &&
                    severity >= severityThreshold)
                {
                    var logId = log.Fields.TryGetValue("externalId", out var externalIdObj)
                                ? externalIdObj?.ToString()
                                : "Unknown";

                    alerts.Add($"Severity {severity} exceeded threshold for external log id: {logId}!");
                }
            }

            var response = new JsonResponse
            {
                Query = query,
                TotalLogs = results.Count,
                DuplicateCount = results.DuplicateCount,
                Logs = results.Logs.Select(log => log.Fields).ToList(),
                Alerts = alerts
            };

            Console.WriteLine(JsonUtility.Serialize(response));
        }
    }
}

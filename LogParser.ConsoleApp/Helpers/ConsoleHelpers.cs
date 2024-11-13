using LogParser.ConsoleApp.Configuration;
using LogParser.Utilities.Models;
using System.Text.Json;

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
            var settings = JsonSerializer.Deserialize<AppSettings>(json);

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

        public static void DisplayResults(QueryResult results, int severityThreshold)
        {
            Console.WriteLine("\n********** Query Result *******************");
            Console.WriteLine($"Total Found Logs: {results.Count}");
            foreach (var record in results.Logs)
            {
                DisplayRecord(record, severityThreshold);
            }
            Console.WriteLine("*****************************************");
        }

        private static void DisplayRecord(CsvLog record, int severityThreshold)
        {
            if (record.Fields.TryGetValue("severity", out var severityObj) &&
                int.TryParse(severityObj?.ToString(), out int severity) &&
                severity >= severityThreshold)
            {
                Console.WriteLine("\n========== ALERT NOTIFICATION ==========");
                Console.WriteLine($"Severity {severity} exceeded threshold!");
                Console.WriteLine("=============================\n");
            }

            var json = JsonSerializer.Serialize(record.Fields, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
        }
    }
}

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
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            var json = File.ReadAllText(filePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);

            if (settings == null)
            {
                throw new InvalidOperationException("Failed to load configuration");
            }

            return settings;
        }

        public static List<string> GetFilePathsFromUser(string defaultFilePath)
        {
            Console.WriteLine("Enter the paths of CSV files, separated by commas. Press Enter to use the default file:");
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
                    Console.WriteLine($"Default file not found: {defaultFilePath}");
                }
            }
            else
            {
                var paths = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var path in paths)
                {
                    var trimmedPath = path.Trim();
                    if (File.Exists(trimmedPath))
                    {
                        filePaths.Add(trimmedPath);
                    }
                    else
                    {
                        Console.WriteLine($"File not found: {trimmedPath}");
                    }
                }
            }

            return filePaths;
        }

        public static int GetSeverityThreshold()
        {
            Console.WriteLine("Please enter the severity threshold: ");
            if (!int.TryParse(Console.ReadLine(), out int severityThreshold))
            {
                Console.WriteLine("Invalid input. Defaulting severity threshold to 3.");
                severityThreshold = 3;
            }

            return severityThreshold;
        }

        public static void DisplayResults(QueryResult results, int severityThreshold)
        {
            Console.WriteLine("\n********** Query Result ****************");
            Console.WriteLine($"Total Records: {results.Count}");
            foreach (var record in results.Records)
            {
                DisplayRecord(record, severityThreshold);
            }
            Console.WriteLine("***************************************");
        }

        private static void DisplayRecord(CsvRecord record, int severityThreshold)
        {
            // Check for alerts based on severity
            if (record.Fields.TryGetValue("severity", out var severityObj) &&
                int.TryParse(severityObj?.ToString(), out int severity) &&
                severity >= severityThreshold)
            {
                Console.WriteLine("\n========== ALERT ==========");
                Console.WriteLine($"Severity {severity} exceeded threshold!");
                Console.WriteLine("===========================\n");
            }

            var json = JsonSerializer.Serialize(record.Fields, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
        }
    }
}

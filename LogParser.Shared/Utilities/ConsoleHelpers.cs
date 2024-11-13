using LogParser.Shared.Configuration;
using System.Text.Json;

namespace LogParser.Shared.Utilities
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

            if (settings == null)
            {
                throw new InvalidOperationException("Failed to load configuration");
            }

            return settings;
        }

        public static List<string> GetFilePathsFromUser(string defaultFilePath)
        {
            Console.WriteLine("Enter the paths of CSV files, separate them by commas. Press enter to use the default file:");
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
        public static void DisplayResults(object results)
        {
            var jsonResult = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine("\n ********** Query Result ****************");
            Console.WriteLine(jsonResult);

            //Console.WriteLine("\n *********** Your Last 3 Query Result History ***********");
            //csvParser.DisplaySavedRecords();
        }
    }
}

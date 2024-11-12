using LogParser.Shared.Utilities;
using System.Text.Json;

namespace LogParser.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConsoleHelpers.LoadConfiguration("appsettings.json");
            var filePaths = ConsoleHelpers.GetFilePathsFromUser(config.DefaultFilePath);

            if (filePaths.Count == 0)
            {
                Console.WriteLine("Exiting the app as no file provided.");
                return;
            }

            var csvParser = new CsvParser(filePaths);

            while (true)
            {
                Console.WriteLine("Enter your query or type 'exit' to quit");
                var query = Console.ReadLine();

                if (query?.ToLower() == "exit")
                {
                    Console.WriteLine("Exiting the app!");
                    break;
                }
                else if (string.IsNullOrEmpty(query))
                {
                    Console.WriteLine("Query cannot be empty!");
                    continue;
                }

                try
                {
                    var results = csvParser.QueryCsv(query);
                    var jsonResult = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });

                    Console.WriteLine("\n ********** Query Result ****************");
                    Console.WriteLine(jsonResult.ToString());

                    //Console.WriteLine("\n *********** Your Last 3 Query Result History ***********");
                    //csvParser.DisplaySavedRecords();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occured: {ex.Message}");
                }
            }
        }
    }
}

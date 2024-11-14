using LogParser.ConsoleApp.Helpers;
using LogParser.Utilities.Data;
using LogParser.Utilities.Models;
using LogParser.Utilities.Services;
using Microsoft.EntityFrameworkCore;

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
                Console.WriteLine("Exiting the app because no file was provided.");
                return;
            }

            var severityThreshold = ConsoleHelpers.GetSeverityThreshold();

            var csvParser = new CsvFileParser();
            var options = new DbContextOptionsBuilder<LogParserDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            var dbContext = new LogParserDbContext(options);
            var queryExecutor = new QueryExecutor(csvParser, dbContext);

            StartQueryLoop(filePaths, queryExecutor, severityThreshold);
        }

        private static void StartQueryLoop(List<string> filePaths, QueryExecutor queryExecutor, int severityThreshold)
        {
            while (true)
            {
                Console.WriteLine("Enter your log query or type exit to quit:");
                var query = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(query))
                {
                    var responseMessage = new JsonResponse
                    {
                        Query = string.Empty,
                        Message = $"Query cannot be empty! Please provide a valid query."
                    };
                    Console.WriteLine(JsonUtility.Serialize(responseMessage));
                    continue;
                }

                if (query.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Exiting the app.");
                    break;
                }

                try
                {
                    var results = queryExecutor.ExecuteQuery(filePaths, query);
                    ConsoleHelpers.DisplayResults(results, query, severityThreshold);
                }
                catch (InvalidOperationException ex)
                {
                    var errorResponse = new JsonResponse
                    {
                        Query = query,
                        Message = $"Error: {ex.Message}. Please adjust your query."
                    };
                    Console.WriteLine(JsonUtility.Serialize(errorResponse));
                }
                catch (Exception ex)
                {
                    var errorResponse = new JsonResponse
                    {
                        Query = query,
                        Message = $"An unexpected error occurred: {ex.Message}"
                    };
                    Console.WriteLine(JsonUtility.Serialize(errorResponse));
                }
            }
        }
    }
}

using LogParser.Data;
using LogParser.Shared.Utilities;
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
                Console.WriteLine("Exiting the app as no file provided.");
                return;
            }

            var csvParser = new CsvFileParser();
            var options = new DbContextOptionsBuilder<LogParserDbContext>()
                  .UseInMemoryDatabase("TestDb")
                  .Options;
            var dbContext = new LogParserDbContext(options);
            var queryExecutor = new QueryExecutor(csvParser, dbContext);

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
                    var results = queryExecutor.ExecuteQuery(filePaths, query);
                    ConsoleHelpers.DisplayResults(results);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occured: {ex.Message}");
                }
            }
        }
    }
}

﻿using LogParser.Data;
using LogParser.Shared.Configuration;
using LogParser.Shared.Models;
using LogParser.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LogParser.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = LoadConfiguration("appsettings.json");
            var filePaths = GetFilePathsFromUser(config.DefaultFilePath);

            if (filePaths.Count == 0)
            {
                Console.WriteLine("Exiting the app as no file provided.");
                return;
            }

            int severityThreshold = GetSeverityThreshold();

            var csvParser = new CsvFileParser();
            var options = new DbContextOptionsBuilder<LogParserDbContext>()
                  .UseInMemoryDatabase("TestDb")
                  .Options;
            var dbContext = new LogParserDbContext(options);
            var queryExecutor = new QueryExecutor(csvParser, dbContext);

            StartQueryLoop(filePaths, queryExecutor, severityThreshold);
        }

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

        public static int GetSeverityThreshold()
        {
            Console.WriteLine("Please enter the severity threshold: ");
            if (!int.TryParse(Console.ReadLine(), out int severityThreshold))
            {
                Console.WriteLine("Invalid input. Defaulting severityThreshold to 3.");
                severityThreshold = 3;
            }

            return severityThreshold;
        }

        private static void StartQueryLoop(List<string> filePaths, QueryExecutor queryExecutor, int severityThreshold)
        {
            while (true)
            {
                Console.WriteLine("Enter your query or type 'exit' to quit");
                var query = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(query))
                {
                    Console.WriteLine("Query cannot be empty!");
                    continue;
                }

                if (query.ToLower() == "exit")
                {
                    Console.WriteLine("Exiting the app!");
                    break;
                }

                try
                {
                    var results = queryExecutor.ExecuteQuery(filePaths, query);
                    DisplayResults(results, severityThreshold);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
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

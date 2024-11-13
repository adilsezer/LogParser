using CsvHelper;
using CsvHelper.Configuration;
using LogParser.Utilities.Models;
using System.Globalization;

namespace LogParser.Utilities.Services
{
    public class CsvFileParser : ICsvFileParser
    {
        public IEnumerable<CsvRecord> ParseCsv(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

                var records = new List<CsvRecord>();
                int idCounter = 1;

                foreach (var row in csv.GetRecords<dynamic>())
                {
                    var csvRecord = new CsvRecord
                    {
                        Id = idCounter++,
                        Fields = ((IDictionary<string, object>)row).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    };
                    records.Add(csvRecord);
                }
                return records;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error happened when we parsing the CSV file, {ex.Message}");
            }
        }
    }
}

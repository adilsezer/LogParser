using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace LogParser.Shared.Utilities
{
    public class CsvFileParser : ICsvFileParser
    {
        public IEnumerable<dynamic> ParseCsv(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
                return csv.GetRecords<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error parsing CSV file: {ex.Message}");
            }
        }
    }
}

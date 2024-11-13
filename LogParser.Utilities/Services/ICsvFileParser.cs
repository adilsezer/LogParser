using LogParser.Utilities.Models;

namespace LogParser.Utilities.Services
{
    public interface ICsvFileParser
    {
        IEnumerable<CsvRecord> ParseCsv(string filePath);
    }
}

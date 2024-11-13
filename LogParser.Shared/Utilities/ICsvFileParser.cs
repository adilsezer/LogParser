using LogParser.Shared.Models;

namespace LogParser.Shared.Utilities
{
    public interface ICsvFileParser
    {
        IEnumerable<CsvRecord> ParseCsv(string filePath);
    }
}

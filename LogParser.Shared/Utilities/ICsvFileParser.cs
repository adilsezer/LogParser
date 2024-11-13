using LogParser.Data;

namespace LogParser.Shared.Utilities
{
    public interface ICsvFileParser
    {
        IEnumerable<CsvRecord> ParseCsv(string filePath);
    }
}

using LogParser.Utilities.Models;

namespace LogParser.Utilities.Services
{
    public interface ICsvFileParser
    {
        IEnumerable<CsvLog> ParseCsv(string filePath);
    }
}

namespace LogParser.Shared.Utilities
{
    public interface ICsvFileParser
    {
        IEnumerable<dynamic> ParseCsv(string filePath);
    }
}

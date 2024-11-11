using LogParser.Shared.Models;

namespace LogParser.Shared.Utilities
{
    public interface IQueryParser
    {
        Func<LogEntry, bool> Parse(string query);
    }
}

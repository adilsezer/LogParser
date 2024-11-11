using LogParser.Shared.Models;

namespace LogParser.Shared.Utilities
{
    public class QueryParser : IQueryParser
    {
        public Func<LogEntry, bool> Parse(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            return log =>
            {
                var logs = new[] { log }.AsQueryable();
                return logs.Where(query).Any();
            };
        }
    }
}

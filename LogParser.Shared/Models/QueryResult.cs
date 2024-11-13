
namespace LogParser.Shared.Models
{
    public class QueryResult
    {
        public int Count { get; set; }
        public List<CsvRecord> Records { get; set; } = new();
    }
}

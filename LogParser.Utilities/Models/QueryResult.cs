
namespace LogParser.Utilities.Models
{
    public class QueryResult
    {
        public int Count { get; set; }
        public List<CsvLog> Logs { get; set; } = [];
    }
}

namespace LogParser.Shared.Models
{
    public class QueryResult
    {
        public int Count { get; set; }
        public List<dynamic> Records { get; set; } = new List<dynamic>();
    }
}

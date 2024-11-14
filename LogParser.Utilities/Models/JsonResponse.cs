namespace LogParser.Utilities.Models
{
    public class JsonResponse
    {
        public string Query { get; set; } = string.Empty;
        public int TotalLogs { get; set; }
        public int DuplicateCount { get; set; }
        public List<Dictionary<string, object>> Logs { get; set; } = new();
        public List<string> Alerts { get; set; } = new();
        public string Message { get; set; } = "Query executed successfully";
    }
}

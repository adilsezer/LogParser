namespace LogParser.Utilities.Models
{
    public class CsvLog
    {
        public int Id { get; set; }
        public Dictionary<string, object> Fields { get; set; } = [];
    }
}

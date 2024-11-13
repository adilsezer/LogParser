namespace LogParser.Utilities.Models
{
    public class CsvRecord
    {
        public int Id { get; set; }
        public Dictionary<string, object> Fields { get; set; } = new();
    }
}

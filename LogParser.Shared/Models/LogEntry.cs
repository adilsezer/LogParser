namespace LogParser.Shared.Models
{
    public class LogEntry
    {
        public string DeviceVendor { get; set; } = string.Empty;
        public string DeviceProduct { get; set; } = string.Empty;
        public int DeviceVersion { get; set; } = 0;
        public string SignatureId { get; set; } = string.Empty;
        public int Severity { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public DateTime Start { get; set; } = DateTime.MinValue;
        public string Rt { get; set; } = string.Empty;
        public string Msg { get; set; } = string.Empty;
        public string Shost { get; set; } = string.Empty;
        public string Smac { get; set; } = string.Empty;
        public string Dhost { get; set; } = string.Empty;
        public string Dmac { get; set; } = string.Empty;
        public string Suser { get; set; } = string.Empty;
        public string Suid { get; set; } = string.Empty;
        public int ExternalId { get; set; } = 0;
        public string Cs1Label { get; set; } = string.Empty;
        public string Cs1 { get; set; } = string.Empty;
        public string Sproc { get; set; } = string.Empty;
        public Dictionary<string, string> AdditionalFields { get; set; } = new();
    }
}

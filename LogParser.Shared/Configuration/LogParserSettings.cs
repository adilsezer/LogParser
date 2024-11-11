namespace LogParser.Shared.Configuration
{
    public class LogParserSettings
    {
        public string DefaultFilePath { get; set; } = string.Empty;
        public int SeverityThreshold { get; set; } = 0;
    }
}

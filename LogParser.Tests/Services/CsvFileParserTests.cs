using CsvHelper;
using CsvHelper.Configuration;
using LogParser.Utilities.Services;

namespace LogParser.Tests.Services
{
    public class CsvFileParserTests : IDisposable
    {
        private readonly string _testFilePath;

        public CsvFileParserTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
        }

        [Fact]
        public void ParseCsv_ValidCsvFile_ReturnsRecords()
        {
            var csvContent = "Id,Name\n1,Test\n2,Example";
            File.WriteAllText(_testFilePath, csvContent);
            var parser = new CsvFileParser();

            var result = parser.ParseCsv(_testFilePath);

            Assert.NotNull(result);
            var records = result.ToList();
            Assert.Equal(2, records.Count);

            Assert.NotNull(records[0]);
            Assert.Equal("1", records[0].Fields["Id"].ToString());
            Assert.Equal("Test", records[0].Fields["Name"].ToString());

            Assert.NotNull(records[1]);
            Assert.Equal("2", records[1].Fields["Id"].ToString());
            Assert.Equal("Example", records[1].Fields["Name"].ToString());
        }

        [Fact]
        public void ParseCsv_MissingFile_ThrowsFileNotFoundException()
        {
            var parser = new CsvFileParser();
            var invalidFilePath = Path.Combine(Path.GetTempPath(), "nonexistent.csv");

            var exception = Assert.Throws<FileNotFoundException>(() => parser.ParseCsv(invalidFilePath));
            Assert.Contains("File not found", exception.Message);
        }

        [Fact]
        public void ParseCsv_EmptyCsvFile_ReturnsEmptyList()
        {
            File.WriteAllText(_testFilePath, "");
            var parser = new CsvFileParser();

            var result = parser.ParseCsv(_testFilePath);

            Assert.Empty(result);
        }

        [Fact]
        public void ParseCsv_ValidCsvWithDifferentCulture_ReturnsRecords()
        {
            var csvContent = "Id;Name\n1;Test\n2;Example"; // Semicolon as delimiter
            File.WriteAllText(_testFilePath, csvContent);
            var parser = new CsvFileParser();

            var result = ParseCsvWithCustomDelimiter();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            var firstRecord = result.ElementAt(0) as IDictionary<string, object>;
            var secondRecord = result.ElementAt(1) as IDictionary<string, object>;

            Assert.NotNull(firstRecord);
            Assert.Equal("1", firstRecord["Id"].ToString());
            Assert.Equal("Test", firstRecord["Name"].ToString());

            Assert.NotNull(secondRecord);
            Assert.Equal("2", secondRecord["Id"].ToString());
            Assert.Equal("Example", secondRecord["Name"].ToString());
        }

        private IEnumerable<dynamic> ParseCsvWithCustomDelimiter()
        {
            using var reader = new StreamReader(_testFilePath);
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };
            using var csv = new CsvReader(reader, config);
            return csv.GetRecords<dynamic>().ToList();
        }

        public void Dispose()
        {
            // Cleaning up test file
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
    }
}

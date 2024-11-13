using CsvHelper;
using CsvHelper.Configuration;
using LogParser.Shared.Utilities;

namespace LogParser.Tests.LogParser.Shared.Tests
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
            Assert.Equal("1", records[0].Id);
            Assert.Equal("Test", records[0].Name);
            Assert.Equal("2", records[1].Id);
            Assert.Equal("Example", records[1].Name);
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
        public void ParseCsv_InvalidCsvFile_ThrowsCsvHelperException()
        {
            var invalidContent = "Id;Name\n1;Test\n2;Example"; // Invalid delimiter
            File.WriteAllText(_testFilePath, invalidContent);
            var parser = new CsvFileParser();

            Assert.Throws<CsvHelperException>(() => parser.ParseCsv(_testFilePath));
        }

        [Fact]
        public void ParseCsv_IncorrectHeaders_ThrowsCsvHelperException()
        {
            var csvContent = "WrongHeader,Name\n1,Test";
            File.WriteAllText(_testFilePath, csvContent);
            var parser = new CsvFileParser();

            Assert.Throws<CsvHelperException>(() => parser.ParseCsv(_testFilePath));
        }

        [Fact]
        public void ParseCsv_ValidCsvWithDifferentCulture_ReturnsRecords()
        {
            var csvContent = "Id;Name\n1;Test\n2;Example"; // Semicolon as delimiter
            File.WriteAllText(_testFilePath, csvContent);
            var parser = new CsvFileParser();

            IEnumerable<dynamic> ParseCsv()
            {
                using var reader = new StreamReader(_testFilePath);
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
                {
                    Delimiter = ";"
                };
                using var csv = new CsvReader(reader, config);
                return csv.GetRecords<dynamic>().ToList();
            }

            var result = ParseCsv();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        public void Dispose()
        {
            // Cleanuing up test file
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
    }
}

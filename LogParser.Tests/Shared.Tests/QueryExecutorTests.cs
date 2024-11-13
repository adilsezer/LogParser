﻿using LogParser.Data;
using LogParser.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Dynamic;

namespace LogParser.Tests.LogParser.Shared.Tests
{
    public class QueryExecutorTests : IDisposable
    {
        private readonly LogParserDbContext _dbContext;
        private readonly Mock<ICsvFileParser> _mockCsvFileParser;

        public QueryExecutorTests()
        {
            var options = new DbContextOptionsBuilder<LogParserDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            _dbContext = new LogParserDbContext(options);

            _mockCsvFileParser = new Mock<ICsvFileParser>();
        }

        [Fact]
        public void ExecuteQuery_ValidQuery_ReturnsExpectedResults()
        {
            var filePaths = new[] { "test.csv" };
            var mockData = CreateMockCsvData(new[] { ("Name", "Jack"), ("Age", "30") });
            _mockCsvFileParser
                .Setup(p => p.ParseCsv(It.IsAny<string>()))
                .Returns(mockData);

            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var query = "Name = 'Jack'";

            var result = queryExecutor.ExecuteQuery(filePaths, query);

            Assert.NotNull(result);
            var resultObject = Assert.IsType<Dictionary<string, object>>(result);
            Assert.Equal(1, resultObject["Count"]);
            Assert.Single((List<dynamic>)resultObject["Records"]);
        }

        [Fact]
        public void ExecuteQuery_InvalidColumn_ReturnsError()
        {
            var filePaths = new[] { "test.csv" };
            var mockData = CreateMockCsvData(new[] { ("Name", "Alice") });
            _mockCsvFileParser
                .Setup(p => p.ParseCsv(It.IsAny<string>()))
                .Returns(mockData);

            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var query = "InvalidColumn = 'Value'";

            var result = queryExecutor.ExecuteQuery(filePaths, query);

            Assert.NotNull(result);
            var resultObject = Assert.IsType<Dictionary<string, object>>(result);
            Assert.Contains("Error", resultObject.Keys);
            Assert.Contains("Columns not found", resultObject["Error"].ToString());
        }

        [Fact]
        public void ExecuteQuery_NoMatchingRecords_ReturnsEmptyResult()
        {
            var filePaths = new[] { "test.csv" };
            var mockData = CreateMockCsvData(new[] { ("Name", "Alice") });
            _mockCsvFileParser
                .Setup(p => p.ParseCsv(It.IsAny<string>()))
                .Returns(mockData);

            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var query = "Name = 'Bob'";

            var result = queryExecutor.ExecuteQuery(filePaths, query);

            Assert.NotNull(result);
            var resultObject = Assert.IsType<Dictionary<string, object>>(result);
            Assert.Equal(0, resultObject["Count"]);
            Assert.Empty((List<dynamic>)resultObject["Records"]);
        }

        [Fact]
        public void ExecuteQuery_SavesMatchingRecordsToDatabase()
        {
            var filePaths = new[] { "test.csv" };
            var mockData = CreateMockCsvData(new[] { ("Name", "Alice"), ("Age", "30") });
            _mockCsvFileParser
                .Setup(p => p.ParseCsv(It.IsAny<string>()))
                .Returns(mockData);

            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var query = "Name = 'Alice'";

            queryExecutor.ExecuteQuery(filePaths, query);

            var records = _dbContext.CsvRecords.ToList();
            Assert.Single(records);
            Assert.Contains("Alice", records.First().JsonData);
        }

        [Fact]
        public void ExecuteQuery_EmptyFilePaths_ThrowsArgumentException()
        {
            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var query = "Name = 'Alice'";

            var exception = Assert.Throws<ArgumentException>(() => queryExecutor.ExecuteQuery(Enumerable.Empty<string>(), query));
            Assert.Equal("No file paths provided", exception.Message);
        }

        [Fact]
        public void ExecuteQuery_NullQuery_ThrowsArgumentNullException()
        {
            var filePaths = new[] { "test.csv" };
            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);

            var exception = Assert.Throws<ArgumentNullException>(() => queryExecutor.ExecuteQuery(filePaths, null));
            Assert.Equal("Query cannot be null", exception.ParamName);
        }

        [Fact]
        public void ExecuteQuery_InvalidQuery_ThrowsArgumentException()
        {
            var filePaths = new[] { "test.csv" };
            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var invalidQuery = "INVALID_QUERY_SYNTAX";

            var exception = Assert.Throws<ArgumentException>(() => queryExecutor.ExecuteQuery(filePaths, invalidQuery));
            Assert.Contains("Invalid query format", exception.Message);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private IEnumerable<dynamic> CreateMockCsvData(params (string Column, string Value)[] columns)
        {
            var expandoObject = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expandoObject;

            foreach (var (column, value) in columns)
            {
                dictionary.Add(column, value);
            }

            return new List<dynamic> { expandoObject };
        }
    }
}
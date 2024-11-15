﻿using LogParser.Utilities.Data;
using LogParser.Utilities.Models;
using LogParser.Utilities.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LogParser.Tests.Services
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
            var mockData = CreateMockCsvData(new[] { ("Name", "Adil"), ("Age", "30") });
            _mockCsvFileParser
                .Setup(p => p.ParseCsv(It.IsAny<string>()))
                .Returns(mockData);

            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var query = "Name = 'Adil'";

            var result = queryExecutor.ExecuteQuery(filePaths, query);

            Assert.NotNull(result);
            var resultObject = Assert.IsType<QueryResult>(result);
            Assert.Equal(1, resultObject.Count);
            Assert.Single(resultObject.Logs);

            var record = resultObject.Logs.First();
            Assert.NotNull(record);
            Assert.Equal("Adil", record.Fields["Name"].ToString());
        }

        [Fact]
        public void ExecuteQuery_InvalidColumn_ThrowsInvalidOperationException()
        {
            var filePaths = new[] { "test.csv" };
            var mockData = CreateMockCsvData(new[] { ("Name", "Adil") });
            _mockCsvFileParser
                .Setup(p => p.ParseCsv(It.IsAny<string>()))
                .Returns(mockData);

            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var query = "InvalidColumn = 'Value'";

            Assert.Throws<InvalidOperationException>(() => queryExecutor.ExecuteQuery(filePaths, query));
        }

        [Fact]
        public void ExecuteQuery_NoMatchingLogs_ReturnsEmptyResult()
        {
            var filePaths = new[] { "test.csv" };
            var mockData = CreateMockCsvData(new[] { ("Name", "Adil") });
            _mockCsvFileParser
                .Setup(p => p.ParseCsv(It.IsAny<string>()))
                .Returns(mockData);

            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var query = "Name = 'Peter'";

            var result = queryExecutor.ExecuteQuery(filePaths, query);

            Assert.NotNull(result);
            var resultObject = Assert.IsType<QueryResult>(result);
            Assert.Equal(0, resultObject.Count);
            Assert.Empty(resultObject.Logs);
        }

        [Fact]
        public void ExecuteQuery_SavesMatchingLogsToDatabase()
        {
            var filePaths = new[] { "test.csv" };
            var mockData = CreateMockCsvData(new[] { ("Name", "Adil"), ("Age", "30") });
            _mockCsvFileParser
                .Setup(p => p.ParseCsv(It.IsAny<string>()))
                .Returns(mockData);

            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var query = "Name = 'Adil'";

            queryExecutor.ExecuteQuery(filePaths, query);

            var logs = _dbContext.CsvLogs.ToList();
            Assert.Single(logs);

            var savedRecord = logs.First();
            Assert.True(savedRecord.Fields.ContainsKey("Name"));
            Assert.Equal("Adil", savedRecord.Fields["Name"].ToString());
        }

        [Fact]
        public void ExecuteQuery_EmptyFilePaths_ThrowsArgumentException()
        {
            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);
            var query = "Name = 'Adil'";

            var exception = Assert.Throws<ArgumentNullException>(() => queryExecutor.ExecuteQuery([], query));
            Assert.Contains("No file paths provided", exception.Message);
        }

        [Fact]
        public void ExecuteQuery_NullQuery_ThrowsArgumentNullException()
        {
            var filePaths = new[] { "test.csv" };
            var queryExecutor = new QueryExecutor(_mockCsvFileParser.Object, _dbContext);

            var exception = Assert.Throws<ArgumentNullException>(() => queryExecutor.ExecuteQuery(filePaths, null));
            Assert.Contains("Query cannot be null", exception.Message);
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

        private IEnumerable<CsvLog> CreateMockCsvData(params (string Column, string Value)[] columns)
        {
            var fields = new Dictionary<string, object>();

            foreach (var (column, value) in columns)
            {
                fields[column] = value;
            }

            return [new CsvLog { Fields = fields }];
        }
    }
}

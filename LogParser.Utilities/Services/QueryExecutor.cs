using LogParser.Utilities.Data;
using LogParser.Utilities.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LogParser.Utilities.Services
{
    public class QueryExecutor
    {
        private readonly ICsvFileParser _csvFileParser;
        private readonly LogParserDbContext _dbContext;

        public QueryExecutor(ICsvFileParser csvFileParser, LogParserDbContext dbContext)
        {
            _csvFileParser = csvFileParser;
            _dbContext = dbContext;
        }

        public QueryResult ExecuteQuery(IEnumerable<string> filePaths, string query)
        {
            if (filePaths == null || !filePaths.Any())
            {
                throw new ArgumentNullException("No file paths provided");
            }
            if (query == null)
            {
                throw new ArgumentNullException("Query cannot be null");
            }

            var queryParser = new QueryParser(query);
            var allData = new List<CsvRecord>();
            var missingColumns = new HashSet<string>(queryParser.Conditions.Select(c => c.Column));

            foreach (var filePath in filePaths)
            {
                var data = _csvFileParser.ParseCsv(filePath);

                if (data.Any())
                {
                    var columnsInFile = data.First().Fields.Keys;
                    missingColumns.ExceptWith(columnsInFile);

                    if (missingColumns.Count == 0)
                    {
                        allData.AddRange(data);
                    }
                }
            }

            if (missingColumns.Count != 0)
            {
                throw new InvalidOperationException($"We couldn't find these wolumns: {string.Join(", ", missingColumns)}");
            }

            var matchingRecords = allData.Where(record => EvaluateConditions(record.Fields, queryParser)).ToList();

            if (matchingRecords.Any())
            {
                SaveRecordsToDatabase(matchingRecords);
            }

            return new QueryResult
            {
                Count = matchingRecords.Count,
                Records = matchingRecords
            };
        }

        public bool EvaluateConditions(Dictionary<string, object> fields, QueryParser queryParser)
        {
            if (queryParser.Conditions.Count == 0)
            {
                return false;
            }

            var result = EvaluateCondition(fields, queryParser.Conditions[0]);

            for (var i = 1; i < queryParser.Conditions.Count; i++)
            {
                var condition = queryParser.Conditions[i];
                var logicalOperator = queryParser.LogicalOperators[i - 1];

                if (logicalOperator.Equals("AND", StringComparison.OrdinalIgnoreCase))
                {
                    result = result && EvaluateCondition(fields, condition);
                }
                else if (logicalOperator.Equals("OR", StringComparison.OrdinalIgnoreCase))
                {
                    result = result || EvaluateCondition(fields, condition);
                }
            }
            return result;
        }

        public bool EvaluateCondition(Dictionary<string, object> fields, (string Column, string Operator, string Value, bool IsNot) condition)
        {
            if (!fields.TryGetValue(condition.Column, out var valueObj) || condition.Value == null)
            {
                return false;
            }

            var value = valueObj?.ToString();
            bool match = condition.Operator switch
            {
                "=" => value != null && MatchWithWildcards(value, condition.Value),
                "!=" => value != null && MatchWithWildcards(value, condition.Value),
                ">" => value != null && CompareValues(value, condition.Value) > 0,
                "<" => value != null && CompareValues(value, condition.Value) < 0,
                ">=" => value != null && CompareValues(value, condition.Value) >= 0,
                "<=" => value != null && CompareValues(value, condition.Value) <= 0,
                _ => throw new NotImplementedException($"This operator {condition.Operator} is not supported.")
            };

            return condition.IsNot ? !match : match;
        }

        private int CompareValues(string value1, string value2)
        {
            if (double.TryParse(value1, out var num1) && double.TryParse(value2, out var num2))
            {
                return num1.CompareTo(num2);
            }
            return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchWithWildcards(string value, string searchString)
        {
            var regexPattern = "^" + Regex.Escape(searchString).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(value, regexPattern);
        }

        public void DisplaySavedRecords()
        {
            var records = _dbContext.CsvRecords.ToList();

            if (records.Count == 0)
            {
                Console.WriteLine("No Saved Records");
            }
            else
            {
                // Displaying all records takes a lot of space, that's why we take 3
                foreach (var record in records.Take(3))
                {
                    var json = JsonSerializer.Serialize(record.Fields, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine($"Id: {record.Id}, Json: {json}");
                }
            }
        }

        public void SaveRecordsToDatabase(IEnumerable<CsvRecord> records)
        {
            if (records == null || !records.Any())
            {
                throw new ArgumentException("No records to save.", nameof(records));
            }

            foreach (var record in records)
            {
                _dbContext.CsvRecords.Add(new CsvRecord { Fields = record.Fields });
            }

            _dbContext.SaveChanges();
        }
    }
}

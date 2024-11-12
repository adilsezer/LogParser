using CsvHelper;
using CsvHelper.Configuration;
using LogParser.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LogParser.Shared.Utilities
{
    public class CsvParser
    {
        private readonly List<string> _filePaths;

        public CsvParser(List<string> filePaths)
        {
            _filePaths = filePaths;
        }

        public IEnumerable<dynamic> ParseCsv(string filePath)
        {
            var records = new List<dynamic>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                records.AddRange(csv.GetRecords<dynamic>().ToList());
            }

            return records;
        }

        public object QueryCsv(string query)
        {
            var queryParser = new QueryParser(query);
            var allData = new List<dynamic>();
            var missingColumns = new HashSet<string>(queryParser.Conditions.Select(c => c.Column));

            foreach (var filePath in _filePaths)
            {
                var data = ParseCsv(filePath);

                if (data.Any())
                {
                    var columnsInFile = ((IDictionary<string, object>)data.First()).Keys;
                    missingColumns.ExceptWith(columnsInFile);

                    if (!missingColumns.Any())
                    {
                        allData.AddRange(data);
                    }
                }
            }

            if (missingColumns.Any())
            {
                return new { Error = $"Columns not found: {string.Join(", ", missingColumns)}" };
            }

            var matchingRecords = allData.Where(row => EvaluateConditions((IDictionary<string, object>)row, queryParser)).ToList();
            var uniqueRecords = new HashSet<string>();

            foreach (var record in matchingRecords)
            {
                var jsonString = JsonSerializer.Serialize(record);
                uniqueRecords.Add(jsonString);
            }

            using (var dbContext = new LogParserDbContext())
            {
                foreach (var json in uniqueRecords)
                {
                    dbContext.CsvRecords.Add(new CsvRecord { JsonData = json });
                }
                dbContext.SaveChanges();
            }

            var result = new
            {
                uniqueRecords.Count,
                Records = uniqueRecords.Select(json => JsonSerializer.Deserialize<dynamic>(json)).ToList()
            };
            return result;
        }

        private bool EvaluateConditions(IDictionary<string, object> rowDict, QueryParser queryParser)
        {
            var result = EvaluateCondition(rowDict, queryParser.Conditions[0]);
            for (var i = 1; i < queryParser.Conditions.Count; i++)
            {
                var condition = queryParser.Conditions[i];
                var logicalOperator = queryParser.LogicalOperators[i - 1];

                if (logicalOperator == "AND")
                {
                    result = result && EvaluateCondition(rowDict, condition);
                }
                else if (logicalOperator == "OR")
                {
                    result = result || EvaluateCondition(rowDict, condition);
                }
            }
            return result;
        }

        private bool EvaluateCondition(IDictionary<string, object> rowDict, (string Column, string Operator, string Value, bool IsNot) condition)
        {
            if (!rowDict.TryGetValue(condition.Column, out var valueObj) || condition.Value == null)
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
                _ => throw new NotImplementedException("Not supported operator")
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
            using (var dbContext = new LogParserDbContext())
            {
                var records = dbContext.CsvRecords.ToList();

                if (records.Count == 0)
                {
                    Console.WriteLine("No Saved Records");
                }
                else
                {
                    foreach (var record in records.Take(3))
                    {
                        Console.WriteLine($"Id: {record.Id}, Json: {record.JsonData}");
                    }
                }
            }
        }
    }
}
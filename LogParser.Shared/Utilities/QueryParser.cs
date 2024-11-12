
using System.Text.RegularExpressions;

namespace LogParser.Shared.Utilities
{
    public class QueryParser
    {
        public List<(string Column, string Operator, string Value, bool IsNot)> Conditions { get; set; }

        public List<string> LogicalOperators { get; set; }

        public QueryParser(string query)
        {
            Conditions = new List<(string, string, string, bool)>();
            LogicalOperators = new List<string>();

            ParseQuery(query);
        }

        private void ParseQuery(string query)
        {
            if (query.Contains("(") || query.Contains(")"))
            {
                throw new ArgumentException("Parenthesis are not supported. Please adjust your query and try again.");
            }

            var conditionPattern = @"(?<Column>\w+)\s*(?<Operator>=|!=)\s*'(?<Value>[^']*)'";
            var logicalOperatorPattern = @"\s*(AND|OR)\s*";

            var conditionMatches = Regex.Matches(query, conditionPattern);
            var logicalOperatorMatches = Regex.Matches(query, logicalOperatorPattern);

            foreach (Match match in conditionMatches)
            {
                var column = match.Groups["Column"].Value;
                var op = match.Groups["Operator"].Value;
                var value = match.Groups["Value"].Value;
                var isNot = op == "!=";

                Conditions.Add((column, op, value, isNot));
            }

            foreach (Match match in logicalOperatorMatches)
            {
                LogicalOperators.Add(match.Value.Trim());
            }

            if (Conditions.Count - 1 != LogicalOperators.Count)
            {
                throw new ArgumentException("Invalid query format");
            }
        }
    }
}

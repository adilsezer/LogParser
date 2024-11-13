using LogParser.Utilities.Services;

namespace LogParser.Tests.Services
{
    public class QueryParserTests
    {
        [Fact]
        public void Constructor_ValidQuery_ParsesConditionsAndOperators()
        {
            var query = "Name = 'Adil' AND Age > '30' OR Country = 'LTU'";

            var queryParser = new QueryParser(query);

            Assert.NotNull(queryParser.Conditions);
            Assert.NotNull(queryParser.LogicalOperators);

            Assert.Equal(3, queryParser.Conditions.Count);
            Assert.Equal(2, queryParser.LogicalOperators.Count);

            Assert.Equal("Name", queryParser.Conditions[0].Column);
            Assert.Equal("=", queryParser.Conditions[0].Operator);
            Assert.Equal("Adil", queryParser.Conditions[0].Value);
            Assert.False(queryParser.Conditions[0].IsNot);

            Assert.Equal("Age", queryParser.Conditions[1].Column);
            Assert.Equal(">", queryParser.Conditions[1].Operator);
            Assert.Equal("30", queryParser.Conditions[1].Value);
            Assert.False(queryParser.Conditions[1].IsNot);

            Assert.Equal("Country", queryParser.Conditions[2].Column);
            Assert.Equal("=", queryParser.Conditions[2].Operator);
            Assert.Equal("LTU", queryParser.Conditions[2].Value);
            Assert.False(queryParser.Conditions[2].IsNot);

            Assert.Equal("AND", queryParser.LogicalOperators[0]);
            Assert.Equal("OR", queryParser.LogicalOperators[1]);
        }

        [Fact]
        public void Constructor_InvalidQuery_ThrowsArgumentException()
        {
            var invalidQuery = "INVALID QUERY";

            var exception = Assert.Throws<ArgumentException>(() => new QueryParser(invalidQuery));
            Assert.Contains("Invalid query format", exception.Message);
        }

        [Fact]
        public void Constructor_QueryWithParentheses_ThrowsArgumentException()
        {
            var queryWithParentheses = "(Name = 'Jack') AND (Age > '30')";

            var exception = Assert.Throws<ArgumentException>(() => new QueryParser(queryWithParentheses));
            Assert.Contains("Parentheses are not supported", exception.Message);
        }

        [Fact]
        public void Constructor_EmptyQuery_ThrowsArgumentException()
        {
            var emptyQuery = "";

            var exception = Assert.Throws<ArgumentException>(() => new QueryParser(emptyQuery));
            Assert.Contains("Query cannot be empty", exception.Message);
        }

        [Fact]
        public void Constructor_QueryWithNotOperator_ParsesCorrectly()
        {
            var query = "Name != 'Alice'";

            var queryParser = new QueryParser(query);

            Assert.Single(queryParser.Conditions);
            var condition = queryParser.Conditions.First();
            Assert.Equal("Name", condition.Column);
            Assert.Equal("!=", condition.Operator);
            Assert.Equal("Alice", condition.Value);
            Assert.True(condition.IsNot);
        }

        [Fact]
        public void Constructor_QueryWithoutLogicalOperators_ParsesSingleCondition()
        {
            var query = "Name = 'Milda'";

            var queryParser = new QueryParser(query);

            Assert.Single(queryParser.Conditions);
            Assert.Empty(queryParser.LogicalOperators);

            var condition = queryParser.Conditions.First();
            Assert.Equal("Name", condition.Column);
            Assert.Equal("=", condition.Operator);
            Assert.Equal("Milda", condition.Value);
            Assert.False(condition.IsNot);
        }

        [Fact]
        public void Constructor_QueryWithMultipleConditionsAndMixedOperators_ParsesCorrectly()
        {
            // Arrange
            var query = "Name != 'Jack' OR Age >= '30' AND Country = 'LTU'";

            // Act
            var queryParser = new QueryParser(query);

            // Assert
            Assert.Equal(3, queryParser.Conditions.Count);
            Assert.Equal(2, queryParser.LogicalOperators.Count);

            Assert.Equal("Name", queryParser.Conditions[0].Column);
            Assert.Equal("!=", queryParser.Conditions[0].Operator);
            Assert.Equal("Jack", queryParser.Conditions[0].Value);
            Assert.True(queryParser.Conditions[0].IsNot);

            Assert.Equal("Age", queryParser.Conditions[1].Column);
            Assert.Equal(">=", queryParser.Conditions[1].Operator);
            Assert.Equal("30", queryParser.Conditions[1].Value);
            Assert.False(queryParser.Conditions[1].IsNot);

            Assert.Equal("Country", queryParser.Conditions[2].Column);
            Assert.Equal("=", queryParser.Conditions[2].Operator);
            Assert.Equal("LTU", queryParser.Conditions[2].Value);
            Assert.False(queryParser.Conditions[2].IsNot);

            Assert.Equal("OR", queryParser.LogicalOperators[0]);
            Assert.Equal("AND", queryParser.LogicalOperators[1]);
        }

        [Fact]
        public void Constructor_QueryWithExtraSpaces_ParsesCorrectly()
        {
            // Arrange
            var query = "  Name  =  'Jack'   AND   Age  >  '30'  ";

            // Act
            var queryParser = new QueryParser(query);

            // Assert
            Assert.Equal(2, queryParser.Conditions.Count);
            Assert.Single(queryParser.LogicalOperators);

            Assert.Equal("Name", queryParser.Conditions[0].Column);
            Assert.Equal("=", queryParser.Conditions[0].Operator);
            Assert.Equal("Jack", queryParser.Conditions[0].Value);
            Assert.False(queryParser.Conditions[0].IsNot);

            Assert.Equal("Age", queryParser.Conditions[1].Column);
            Assert.Equal(">", queryParser.Conditions[1].Operator);
            Assert.Equal("30", queryParser.Conditions[1].Value);
            Assert.False(queryParser.Conditions[1].IsNot);

            Assert.Equal("AND", queryParser.LogicalOperators[0]);
        }
    }
}
namespace CosmosSqlAntlr.Tests
{
    using Antlr4.Runtime;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LiteralsTests : ParserTests
    {
        [TestMethod]
        public void Null()
        {
            LiteralsTests.ValidateLiteral("null");
        }

        [TestMethod]
        public void Undefined()
        {
            LiteralsTests.ValidateLiteral("undefined");
        }

        [TestMethod]
        [DataRow("true")]
        [DataRow("false")]
        public void Booleans(string inputString)
        {
            LiteralsTests.ValidateLiteral(inputString);
        }

        [TestMethod]
        [DataRow("0", DisplayName = "Zero")]
        [DataRow("1", DisplayName = "Positive Integer")]
        [DataRow("-1", DisplayName = "Negative Integer")]
        [DataRow("0.0", DisplayName = "Fractional")]
        [DataRow("10E23", DisplayName = "Uppercase Exponent")]
        [DataRow("10e23", DisplayName = "Lowercase Exponent")]
        [DataRow("10e-23", DisplayName = "Lowercase Negative Exponent")]
        public void NumbersPositive(string inputString)
        {
            LiteralsTests.ValidateLiteral(inputString);
        }

        [TestMethod]
        [DataRow("''", DisplayName = "empty string")]
        [DataRow("'a'", DisplayName = "single character string")]
        [DataRow("'Hello World'", DisplayName = "string with space")]
        //[DataRow(@"'\""\\\/\b\f\n\r\t\u0000'", DisplayName = "escape characters")]
        [DataRow("\"\"", DisplayName = "double quote")]
        public void StringsPositive(string inputString)
        {
            LiteralsTests.ValidateLiteral(inputString);
        }

        private static void ValidateLiteral(string literal)
        {
            string query = $"SELECT VALUE {literal}";
            ParserTests.Validate(query);
        }

        private static void InvalidateLiteral(string literal)
        {
            string query = $"SELECT VALUE {literal}";
            ParserTests.Invalidate(query);
        }
    }
}

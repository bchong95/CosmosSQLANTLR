namespace CosmosSqlAntlr.Tests
{
    using CosmosSqlAntlr.Ast;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public abstract class ParserTests
    {
        protected static void Validate(string query)
        {
            Assert.IsNotNull(query);

            if (!SqlQuery.TryParse(query, out SqlQuery parsedQuery))
            {
                Assert.Fail($"Failed to parse query: {query}");
            }

            string normalizedQuery = NormalizeText(query);
            string normalizedToString = NormalizeText(parsedQuery.ToString());
            Assert.AreEqual(normalizedQuery, normalizedToString);
        }

        protected static void Invalidate(string query)
        {
            Assert.IsNotNull(query);

            Assert.IsFalse(
                SqlQuery.TryParse(query, out _),
                $"Expected failure to parse query: {query}");
        }

        private static string NormalizeText(string text)
        {
            return text
                .ToLower()
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace("asc", string.Empty)
                .Replace("desc", string.Empty)
                .Replace("'", "\"")
                .Replace(" ", string.Empty)
                .Replace("+", string.Empty);
        }
    }
}

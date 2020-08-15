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

            string parsedQueryText = parsedQuery.ToString();
            if (!SqlQuery.TryParse(parsedQueryText, out SqlQuery reparsedQuery))
            {
                Assert.Fail($"Failed to parse query: {parsedQueryText}");
            }

            Assert.AreEqual(parsedQuery.ToString(), reparsedQuery.ToString());
        }

        protected static void Invalidate(string query)
        {
            Assert.IsNotNull(query);

            Assert.IsFalse(
                SqlQuery.TryParse(query, out _),
                $"Expected failure to parse query: {query}");
        }
    }
}

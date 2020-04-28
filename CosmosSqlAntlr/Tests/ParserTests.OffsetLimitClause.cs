namespace CosmosSqlAntlr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    public partial class ParserTests
    {
        [TestMethod]
        public void OffsetLimit()
        {
            ParserTests.ValidateOffsetLimit("OFFSET 10 LIMIT 10");

            ParserTests.InvalidateOffsetLimit("OFFSET 'asdf' LIMIT 10");
            ParserTests.InvalidateOffsetLimit("OFFSET 10 ");
            ParserTests.InvalidateOffsetLimit("LIMIT 10 ");
        }

        private static void ValidateOffsetLimit(string offsetLimitClause)
        {
            string query = $"SELECT * {offsetLimitClause}";
            ParserTests.Validate(query);
        }

        private static void InvalidateOffsetLimit(string offsetLimitClause)
        {
            string query = $"SELECT * {offsetLimitClause}";
            ParserTests.Invalidate(query);
        }
    }
}

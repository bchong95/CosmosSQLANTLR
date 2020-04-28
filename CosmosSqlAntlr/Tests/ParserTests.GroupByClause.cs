namespace CosmosSqlAntlr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    public partial class ParserTests
    {
        [TestMethod]
        public void SingleGroupBy()
        {
            ParserTests.ValidateGroupBy("GROUP BY 1");
            ParserTests.InvalidateGroupBy("GROUP BY ");
            ParserTests.InvalidateGroupBy("GROUPBY 1");
        }

        [TestMethod]
        public void MultiGroupBy()
        {
            ParserTests.ValidateGroupBy("GROUP BY 1, 2, 3");
            ParserTests.ValidateGroupBy("GROUP BY 1, 2");
            ParserTests.InvalidateGroupBy("GROUP BY 1,");
        }

        private static void ValidateGroupBy(string groupByClause)
        {
            string query = $"SELECT * {groupByClause}";
            ParserTests.Validate(query);
        }

        private static void InvalidateGroupBy(string groupByClause)
        {
            string query = $"SELECT * {groupByClause}";
            ParserTests.Invalidate(query);
        }
    }
}

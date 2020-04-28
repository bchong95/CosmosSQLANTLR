namespace CosmosSqlAntlr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    public partial class ParserTests
    {
        [TestMethod]
        public void SingleOrderBy()
        {
            ParserTests.ValidateOrderBy("ORDER BY 1");
            ParserTests.ValidateOrderBy("ORDER BY 1 asc");
            ParserTests.ValidateOrderBy("ORDER BY 1 DESC");
            ParserTests.InvalidateOrderBy("ORDERBY 1");
        }

        [TestMethod]
        public void MultiOrderBy()
        {
            ParserTests.ValidateOrderBy("ORDER BY 1, 2, 3");
            ParserTests.ValidateOrderBy("ORDER BY 1, 2 DESC, 3");
            ParserTests.ValidateOrderBy("ORDER BY 1 ASC, 2 DESC, 3 ASC");
            ParserTests.InvalidateOrderBy("ORDER BY 1 ASC,");
        }

        private static void ValidateOrderBy(string orderByClause)
        {
            string query = $"SELECT * {orderByClause}";
            ParserTests.Validate(query);
        }

        private static void InvalidateOrderBy(string orderByClause)
        {
            string query = $"SELECT * {orderByClause}";
            ParserTests.Invalidate(query);
        }
    }
}

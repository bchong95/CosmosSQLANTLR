namespace CosmosSqlAntlr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    public partial class ParserTests
    {
        [TestMethod]
        public void WhereClause()
        {
            ParserTests.ValidateWhere("WHERE true");
            ParserTests.InvalidateWhere("WHERE true, true");
        }

        private static void ValidateWhere(string whereClause)
        {
            string query = $"SELECT * {whereClause}";
            ParserTests.Validate(query);
        }

        private static void InvalidateWhere(string whereClause)
        {
            string query = $"SELECT * {whereClause}";
            ParserTests.Invalidate(query);
        }
    }
}

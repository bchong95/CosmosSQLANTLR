namespace CosmosSqlAntlr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class ParserTests
    {
        [TestMethod]
        public void SelectStar()
        {
            ParserTests.Validate("SELECT *");
            ParserTests.Invalidate("SELECT");
        }

        [TestMethod]
        public void SelectList()
        {
            ParserTests.Validate("SELECT 1, 2, 3");
            ParserTests.Invalidate("SELECT 1,");
        }

        [TestMethod]
        public void SelectValue()
        {
            ParserTests.Validate("SELECT VALUE 1");
            ParserTests.Invalidate("SELECT VALUE 1, 2");
            ParserTests.Invalidate("SELECTVALUE 1");
        }
    }
}

namespace CosmosSqlAntlr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class ParserTests
    {
        [TestMethod]
        public void SelectStar()
        {
            ParserTests.Validate("SELECT *");
        }
    }
}

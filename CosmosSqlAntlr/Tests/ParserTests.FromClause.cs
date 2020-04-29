namespace CosmosSqlAntlr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Linq;

    public partial class ParserTests
    {
        private static readonly string baseInputPathExpression = "c";
        private static readonly string recursiveInputPathExpression = "c.age";
        private static readonly string numberPathExpression = "c.arr[5]";
        private static readonly string stringPathExpression = "c.blah['asdf']";
        private static readonly string[] pathExpressions = new string[]
        {
                baseInputPathExpression,
                recursiveInputPathExpression,
                numberPathExpression,
                stringPathExpression,
        };

        private static readonly string[] inputPathCollections = pathExpressions;
        private static readonly string literalArrayCollection = "[1, 2, 3]";
        private static readonly string subqueryCollection = "(SELECT * FROM c)";
        private static readonly string[] collections = new string[]
        {
                literalArrayCollection,
                subqueryCollection,
        }.Concat(inputPathCollections).ToArray();

        [TestMethod]
        public void AliasedCollection()
        {
            foreach (string collection in collections)
            {
                foreach (bool useAlias in new bool[] { false, true })
                {
                    ParserTests.ValidateFromClause($"FROM {collection} {(useAlias ? "AS asdf" : string.Empty)}");
                }
            }
        }

        [TestMethod]
        public void ArrayIteratorCollection()
        {
            foreach (string collection in collections)
            {
                ParserTests.ValidateFromClause($"FROM item IN {collection}");
            }
        }

        [TestMethod]
        public void JoinCollection()
        {
            ParserTests.ValidateFromClause($"FROM c JOIN d in c.children");
        }

        private static void ValidateFromClause(string fromClause)
        {
            string query = $"SELECT * {fromClause}";
            ParserTests.Validate(query);
        }
    }
}

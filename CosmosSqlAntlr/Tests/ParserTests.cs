namespace CosmosSqlAntlr.Tests
{
    using Antlr4.Runtime;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public partial class ParserTests
    {
        private static void Validate(string query)
        {
            Assert.IsNotNull(query);
            AntlrInputStream str = new AntlrInputStream(query);
            sqlLexer lexer = new sqlLexer(str);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            sqlParser parser = new sqlParser(tokens);
            ErrorListener<IToken> listener = new ErrorListener<IToken>(parser, lexer, tokens);
            parser.AddErrorListener(listener);
            sqlParser.ProgramContext tree = parser.program();
            Assert.IsFalse(listener.had_error, $"Parser ran into error: {tree.OutputTree(tokens)}.");
            Console.WriteLine(tree.OutputTree(tokens));
        }

        private static void Invalidate(string query)
        {
            Assert.IsNotNull(query);
            AntlrInputStream str = new AntlrInputStream(query);
            sqlLexer lexer = new sqlLexer(str);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            sqlParser parser = new sqlParser(tokens);
            ErrorListener<IToken> listener = new ErrorListener<IToken>(parser, lexer, tokens);
            parser.AddErrorListener(listener);
            sqlParser.ProgramContext tree = parser.program();
            Assert.IsTrue(listener.had_error, $"Parser didn't have error.: {tree.OutputTree(tokens)}.");
            Console.WriteLine(tree.OutputTree(tokens));
        }
    }
}
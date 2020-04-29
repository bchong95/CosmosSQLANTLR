namespace CosmosSqlAntlr.Tests
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Microsoft.Azure.Cosmos.Sql;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Runtime.ExceptionServices;

    [TestClass]
    public abstract class ParserTests
    {
        protected static void Validate(string query)
        {
            Assert.IsNotNull(query);
            AntlrInputStream str = new AntlrInputStream(query);
            sqlLexer lexer = new sqlLexer(str);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            sqlParser parser = new sqlParser(tokens);
            ErrorListener<IToken> listener = new ErrorListener<IToken>(parser, lexer, tokens);
            parser.AddErrorListener(listener);
            sqlParser.ProgramContext tree = parser.program();
            Assert.IsFalse(listener.had_error, $"Parser ran into error: '{tree.OutputTree(tokens)}' for query: {query}");
            Console.WriteLine(tree.OutputTree(tokens));
            SqlObject sqlObject = new CstToAstVisitor().Visit(tree);
            string normalizedQuery = NormalizeText(query);
            string normalizedToString = NormalizeText(sqlObject.ToString());
            Assert.AreEqual(normalizedQuery, normalizedToString);
        }

        protected static void Invalidate(string query)
        {
            Assert.IsNotNull(query);
            AntlrInputStream str = new AntlrInputStream(query);
            sqlLexer lexer = new sqlLexer(str);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            sqlParser parser = new sqlParser(tokens);
            parser.ErrorHandler = new ThrowExceptionOnErrors();
            ErrorListener<IToken> listener = new ErrorListener<IToken>(parser, lexer, tokens);
            parser.AddErrorListener(listener);
            try
            {
                sqlParser.ProgramContext tree = parser.program();
                Assert.IsTrue(listener.had_error, $"Parser didn't have error: '{tree.OutputTree(tokens)}' for query: {query}");
                Console.WriteLine(tree.OutputTree(tokens));
            }
            catch (Exception ex)
            {
            }
        }

        private sealed class ThrowExceptionOnErrors : IAntlrErrorStrategy
        {
            public bool InErrorRecoveryMode(Parser recognizer)
            {
                return false;
            }

            public void Recover(Parser recognizer, RecognitionException e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }

            [return: NotNull]
            public IToken RecoverInline(Parser recognizer)
            {
                throw new NotSupportedException("can not recover.");
            }

            public void ReportError(Parser recognizer, RecognitionException e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }

            public void ReportMatch(Parser recognizer)
            {
                // Do nothing
            }

            public void Reset(Parser recognizer)
            {
                // Do nothing
            }

            public void Sync(Parser recognizer)
            {
                // Do nothing
            }
        }

        private static string NormalizeText(string text)
        {
            return text
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .ToLower()
                .Replace("'", "\"")
                .Replace(" ", string.Empty)
                .Replace("+", string.Empty);
        }
    }
}

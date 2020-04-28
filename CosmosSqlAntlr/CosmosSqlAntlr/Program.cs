﻿// Template generated code from Antlr4BuildTasks.Template v 3.0
namespace CosmosSqlAntlr
{
    using Antlr4.Runtime;

    public class Program
    {
        static void Try(string input)
        {
            var str = new AntlrInputStream(input);
            var lexer = new sqlLexer(str);
            var tokens = new CommonTokenStream(lexer);
            var parser = new sqlParser(tokens);
            var listener = new ErrorListener<IToken>(parser, lexer, tokens);
            parser.AddErrorListener(listener);
            var tree = parser.program();
            if (listener.had_error)
            {
                System.Console.WriteLine("error in parse.");
                System.Console.WriteLine(tokens.OutputTokens());
                System.Console.WriteLine(tree.OutputTree(tokens));
            }
            else
            {
                System.Console.WriteLine("parse completed.");
                System.Console.WriteLine(tokens.OutputTokens());
                System.Console.WriteLine(tree.OutputTree(tokens));
                //var visitor = new CalculatorVisitor();
                //visitor.Visit(tree);
            }
        }

        static void Main(string[] args)
        {
            Try("SELECT *");
        }
    }
}

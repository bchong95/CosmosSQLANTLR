namespace CosmosSqlAntlr
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Sql;
    using Microsoft.CodeAnalysis.VisualBasic.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Converts from ANTLR's CST to a CosmosDB SQL AST.
    /// The main difference is that a CST Context will contain every token like array start and delimiters, which is too verbose for most usecases.
    /// The children of a CST are also untyped (generic IParseTree), which doesn't give the contraints we need.
    /// In general the ANTLR parser could in theory generate an incorrect CST if the grammar file has a mistake,
    /// while and AST is strongly typed and any AST is a valid grammar (as long as the AST is defined correctly).
    /// </summary>
    internal sealed class CstToAstVisitor : sqlBaseVisitor<SqlObject>
    {
        private static readonly IReadOnlyDictionary<string, SqlBinaryScalarOperatorKind> binaryOperatorKindLookup = new Dictionary<string, SqlBinaryScalarOperatorKind>(StringComparer.OrdinalIgnoreCase)
        {
            { "+", SqlBinaryScalarOperatorKind.Add },
            { "AND", SqlBinaryScalarOperatorKind.And },
            { "&", SqlBinaryScalarOperatorKind.BitwiseAnd },
            { "|", SqlBinaryScalarOperatorKind.BitwiseOr },
            { "^", SqlBinaryScalarOperatorKind.BitwiseXor },
            { "/", SqlBinaryScalarOperatorKind.Divide },
            { "=", SqlBinaryScalarOperatorKind.Equal },
            { ">", SqlBinaryScalarOperatorKind.GreaterThan },
            { ">=", SqlBinaryScalarOperatorKind.GreaterThanOrEqual },
            { "<", SqlBinaryScalarOperatorKind.LessThan },
            { "<=", SqlBinaryScalarOperatorKind.LessThanOrEqual },
            { "%", SqlBinaryScalarOperatorKind.Modulo },
            { "*", SqlBinaryScalarOperatorKind.Multiply },
            { "!=", SqlBinaryScalarOperatorKind.NotEqual },
            { "OR", SqlBinaryScalarOperatorKind.Or },
            { "||", SqlBinaryScalarOperatorKind.StringConcat },
            { "-", SqlBinaryScalarOperatorKind.Subtract },
        };

        private static readonly IReadOnlyDictionary<string, SqlUnaryScalarOperatorKind> unaryOperatorKindLookup = new Dictionary<string, SqlUnaryScalarOperatorKind>(StringComparer.OrdinalIgnoreCase)
        {

            { "~", SqlUnaryScalarOperatorKind.BitwiseNot },
            { "-", SqlUnaryScalarOperatorKind.Minus },
            { "NOT", SqlUnaryScalarOperatorKind.Not },
            { "+", SqlUnaryScalarOperatorKind.Plus },
        };

        public override SqlObject VisitProgram([NotNull] sqlParser.ProgramContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.ChildCount == 2);

            IParseTree queryContext = context.children[0];
            return this.Visit(queryContext);
        }

        public override SqlObject VisitSql_query([NotNull] sqlParser.Sql_queryContext context)
        {
            Contract.Requires(context != null);

            SqlSelectClause sqlSelectClause = default;
            SqlFromClause sqlFromClause = default;
            SqlWhereClause sqlWhereClause = default;
            SqlOrderbyClause sqlOrderByClause = default;
            SqlGroupByClause sqlGroupByClause = default;
            SqlOffsetLimitClause sqlOffsetLimitClause = default;
            foreach (IParseTree parseTree in context.children)
            {
                SqlObject clause = this.Visit(parseTree);
                switch (clause)
                {
                    case SqlSelectClause select:
                        sqlSelectClause = select;
                        break;

                    case SqlFromClause from:
                        sqlFromClause = from;
                        break;

                    case SqlWhereClause where:
                        sqlWhereClause = where;
                        break;

                    case SqlOrderbyClause orderBy:
                        sqlOrderByClause = orderBy;
                        break;

                    case SqlGroupByClause groupBy:
                        sqlGroupByClause = groupBy;
                        break;

                    case SqlOffsetLimitClause offsetLimit:
                        sqlOffsetLimitClause = offsetLimit;
                        break;

                    default:
                        throw new UnknownSqlObjectException(clause);
                }
            }

            return SqlQuery.Create(
                sqlSelectClause,
                sqlFromClause,
                sqlWhereClause,
                sqlGroupByClause,
                sqlOrderByClause,
                sqlOffsetLimitClause);
        }

        public override SqlObject VisitSelect_clause([NotNull] sqlParser.Select_clauseContext context)
        {
            SqlSelectSpec sqlSelectSpec = default;
            SqlTopSpec sqlTopSpec = default;
            bool distinct = default;

            foreach (IParseTree parseTree in context.children)
            {
                if (parseTree is TerminalNodeImpl terminalNode)
                {
                    switch (terminalNode.Symbol.Type)
                    {
                        case sqlParser.K_DISTINCT:
                            distinct = true;
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    SqlObject sqlObject = this.Visit(parseTree);
                    if (sqlObject != null)
                    {
                        switch (sqlObject)
                        {
                            case SqlSelectSpec selectSpec:
                                sqlSelectSpec = selectSpec;
                                break;

                            case SqlTopSpec topSpec:
                                sqlTopSpec = topSpec;
                                break;

                            default:
                                throw new UnknownSqlObjectException(sqlObject);
                        }
                    }
                }
            }

            return SqlSelectClause.Create(sqlSelectSpec, sqlTopSpec, distinct);
        }

        public override SqlObject VisitSelect_star_spec([NotNull] sqlParser.Select_star_specContext context)
        {
            Contract.Requires(context != null);

            return SqlSelectStarSpec.Create();
        }

        public override SqlObject VisitSelect_value_spec([NotNull] sqlParser.Select_value_specContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.ChildCount == 2);

            IParseTree scalarExpressionContext = context.children[1];
            SqlScalarExpression scalarExpression = (SqlScalarExpression)this.Visit(scalarExpressionContext);
            SqlSelectValueSpec sqlSelectValueSpec = SqlSelectValueSpec.Create(scalarExpression);
            return sqlSelectValueSpec;
        }

        public override SqlObject VisitSelect_list_spec([NotNull] sqlParser.Select_list_specContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.ChildCount >= 1);

            List<SqlSelectItem> sqlSelectItems = new List<SqlSelectItem>();
            foreach (IParseTree parseTree in context.children)
            {
                if (parseTree is ParserRuleContext parserRuleContext)
                {
                    SqlObject sqlObject = this.Visit(parserRuleContext);
                    Contract.Requires(sqlObject != null);

                    sqlSelectItems.Add((SqlSelectItem)sqlObject);
                }
            }

            return SqlSelectListSpec.Create(sqlSelectItems);
        }

        public override SqlObject VisitSelect_item([NotNull] sqlParser.Select_itemContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires((context.ChildCount == 1) || (context.ChildCount == 3));

            SqlScalarExpression sqlScalarExpression = (SqlScalarExpression)this.Visit(context.children[0]);
            SqlIdentifier alias;
            if (context.ChildCount == 3)
            {
                alias = SqlIdentifier.Create(context.children[2].GetText());
            }
            else
            {
                alias = default;
            }

            return SqlSelectItem.Create(sqlScalarExpression, alias);
        }

        public override SqlObject VisitTop_spec([NotNull] sqlParser.Top_specContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.ChildCount == 2);

            Number64 topCount = CstToAstVisitor.GetNumber64ValueFromNode(context.children[1]);
            return SqlTopSpec.Create(SqlNumberLiteral.Create(topCount));
        }

        public override SqlObject VisitArrayCreateScalarExpression([NotNull] sqlParser.ArrayCreateScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.ChildCount >= 2); // start array and end array tokens

            List<SqlScalarExpression> arrayItems = new List<SqlScalarExpression>();
            for (int i = 1; i < context.children.Count - 1; i++)
            {
                IParseTree arrayToken = context.children[i];
                if (arrayToken is ParserRuleContext parserRuleContext)
                {
                    arrayItems.Add((SqlScalarExpression)this.Visit(parserRuleContext));
                }
            }

            return SqlArrayCreateScalarExpression.Create(arrayItems);
        }

        public override SqlObject VisitArrayScalarExpression([NotNull] sqlParser.ArrayScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.ChildCount == 4); // K_ARRAY '(' sql_query ')'

            SqlQuery sqlQuery = (SqlQuery)this.Visit(context.children[2]);
            return SqlArrayScalarExpression.Create(sqlQuery);
        }

        public override SqlObject VisitBetweenScalarExpression([NotNull] sqlParser.BetweenScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression K_NOT? K_BETWEEN scalar_expression K_AND scalar_expression
            Contract.Requires((context.ChildCount == 5) || (context.ChildCount == 6));

            SqlScalarExpression needle = (SqlScalarExpression)this.Visit(context.children[0]);
            bool not = context.ChildCount == 6;
            SqlScalarExpression start = (SqlScalarExpression)this.Visit(context.children[3 - (not ? 0 : 1)]);
            SqlScalarExpression end = (SqlScalarExpression)this.Visit(context.children[5 - (not ? 0 : 1)]);

            return SqlBetweenScalarExpression.Create(needle, start, end, not);
        }

        public override SqlObject VisitBinaryScalarExpression([NotNull] sqlParser.BinaryScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression binary_operator scalar_expression
            Contract.Requires(context.ChildCount == 3);

            SqlScalarExpression left = (SqlScalarExpression)this.Visit(context.children[0]);
            if (!CstToAstVisitor.binaryOperatorKindLookup.TryGetValue(context.children[1].GetText(), out SqlBinaryScalarOperatorKind operatorKind))
            {
                throw new ArgumentOutOfRangeException($"Unknown binary operator: {context.children[1].GetText()}.");
            }

            SqlScalarExpression right = (SqlScalarExpression)this.Visit(context.children[2]);

            return SqlBinaryScalarExpression.Create(operatorKind, left, right);
        }

        public override SqlObject VisitCoalesceScalarExpression([NotNull] sqlParser.CoalesceScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression '??' scalar_expression
            Contract.Requires(context.ChildCount == 3);

            SqlScalarExpression left = (SqlScalarExpression)this.Visit(context.children[0]);
            SqlScalarExpression right = (SqlScalarExpression)this.Visit(context.children[2]);

            return SqlCoalesceScalarExpression.Create(left, right);
        }

        public override SqlObject VisitConditionalScalarExpression([NotNull] sqlParser.ConditionalScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression '?' scalar_expression ':' scalar_expression
            Contract.Requires(context.ChildCount == 5);

            SqlScalarExpression condition = (SqlScalarExpression)this.Visit(context.children[0]);
            SqlScalarExpression consequent = (SqlScalarExpression)this.Visit(context.children[2]);
            SqlScalarExpression alternative = (SqlScalarExpression)this.Visit(context.children[4]);
            return SqlConditionalScalarExpression.Create(condition, consequent, alternative);
        }

        public override SqlObject VisitExistsScalarExpression([NotNull] sqlParser.ExistsScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // K_EXISTS '(' sql_query ')'
            Contract.Requires(context.ChildCount == 4);

            SqlQuery subquery = (SqlQuery)this.Visit(context.children[2]);
            return SqlExistsScalarExpression.Create(subquery);
        }

        public override SqlObject VisitFunctionCallScalarExpression([NotNull] sqlParser.FunctionCallScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.ChildCount >= 3);
            // (K_UDF '.')? IDENTIFIER '(' (scalar_expression ( ',' scalar_expression )*)? ')'
            bool udf = ((TerminalNodeImpl)context.children[0]).Symbol.Type == sqlParser.K_UDF;
            SqlIdentifier identifier = SqlIdentifier.Create(context.children[0 + (udf ? 2 : 0)].GetText());
            List<SqlScalarExpression> arguments = new List<SqlScalarExpression>();
            for (int i = 2 + (udf ? 2 : 0); i < context.ChildCount - 1; i++)
            {
                IParseTree arrayToken = context.children[i];
                if (arrayToken is ParserRuleContext parserRuleContext)
                {
                    arguments.Add((SqlScalarExpression)this.Visit(parserRuleContext));
                }
            }

            return SqlFunctionCallScalarExpression.Create(identifier, udf, arguments);
        }

        public override SqlObject VisitInScalarExpression([NotNull] sqlParser.InScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression K_NOT? K_IN '(' (scalar_expression ( ',' scalar_expression )*)? ')'
            Contract.Requires(context.ChildCount >= 4);

            SqlScalarExpression needle = (SqlScalarExpression)this.Visit(context.children[0]);
            bool not = ((TerminalNodeImpl)context.children[1]).Symbol.Type == sqlParser.K_NOT;
            List<SqlScalarExpression> searchList = new List<SqlScalarExpression>();
            for (int i = 2 + (not ? 1 : 0); i < context.ChildCount - 1; i++)
            {
                IParseTree arrayToken = context.children[i];
                if (arrayToken is ParserRuleContext parserRuleContext)
                {
                    searchList.Add((SqlScalarExpression)this.Visit(parserRuleContext));
                }
            }

            return SqlInScalarExpression.Create(needle, not, searchList);
        }

        public override SqlObject VisitLiteralScalarExpression([NotNull] sqlParser.LiteralScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.ChildCount == 1);
            Contract.Requires(context.children[0].ChildCount == 1);

            TerminalNodeImpl terminalNode = (TerminalNodeImpl)(context.children[0].GetChild(0));

            SqlLiteralScalarExpression sqlLiteralScalarExpression;
            switch (terminalNode.Symbol.Type)
            {
                case sqlParser.STRING_LITERAL:
                    string value = CstToAstVisitor.GetStringValueFromNode(terminalNode);
                    sqlLiteralScalarExpression = SqlLiteralScalarExpression.Create(SqlStringLiteral.Create(value));
                    break;

                case sqlParser.NUMERIC_LITERAL:
                    Number64 number64 = CstToAstVisitor.GetNumber64ValueFromNode(terminalNode);
                    sqlLiteralScalarExpression = SqlLiteralScalarExpression.Create(SqlNumberLiteral.Create(number64));
                    break;

                case sqlParser.K_TRUE:
                    sqlLiteralScalarExpression = SqlLiteralScalarExpression.Create(SqlBooleanLiteral.Create(true));
                    break;

                case sqlParser.K_FALSE:
                    sqlLiteralScalarExpression = SqlLiteralScalarExpression.Create(SqlBooleanLiteral.Create(false));
                    break;

                case sqlParser.K_NULL:
                    sqlLiteralScalarExpression = SqlLiteralScalarExpression.Create(SqlNullLiteral.Create());
                    break;

                case sqlParser.K_UNDEFINED:
                    sqlLiteralScalarExpression = SqlLiteralScalarExpression.Create(SqlUndefinedLiteral.Create());
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unknown symbol type: {terminalNode.Symbol.Type}");
            }

            return sqlLiteralScalarExpression;
        }

        public override SqlObject VisitMemberIndexerScalarExpression([NotNull] sqlParser.MemberIndexerScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression '[' scalar_expression ']'
            Contract.Requires(context.ChildCount == 4);

            SqlScalarExpression memberExpression = (SqlScalarExpression)this.Visit(context.children[0]);
            SqlScalarExpression indexExpression = (SqlScalarExpression)this.Visit(context.children[2]);

            return SqlMemberIndexerScalarExpression.Create(memberExpression, indexExpression);
        }

        public override SqlObject VisitObjectCreateScalarExpression([NotNull] sqlParser.ObjectCreateScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            //'{' (object_property (',' object_property)*)? '}'
            Contract.Requires(context.ChildCount >= 2);

            List<SqlObjectProperty> properties = new List<SqlObjectProperty>();
            for (int i = 1; i < context.children.Count - 1; i++)
            {
                IParseTree objectToken = context.children[i];
                if (objectToken is ParserRuleContext parserRuleContext)
                {
                    // object_property : STRING_LITERAL ':' scalar_expression
                    string name = CstToAstVisitor.GetStringValueFromNode(parserRuleContext.children[0]);
                    SqlScalarExpression sqlScalarExpression = (SqlScalarExpression)this.Visit(parserRuleContext.children[2]);
                    SqlObjectProperty objectProperty = SqlObjectProperty.Create(SqlPropertyName.Create(name), sqlScalarExpression);
                    properties.Add(objectProperty);
                }
            }

            return SqlObjectCreateScalarExpression.Create(properties);
        }

        public override SqlObject VisitPropertyRefScalarExpressionBase([NotNull] sqlParser.PropertyRefScalarExpressionBaseContext context)
        {
            Contract.Requires(context != null);
            // IDENTIFIER
            Contract.Requires(context.ChildCount == 1);

            return SqlPropertyRefScalarExpression.Create(memberExpression: null, SqlIdentifier.Create(context.children[0].GetText()));
        }

        public override SqlObject VisitPropertyRefScalarExpressionRecursive([NotNull] sqlParser.PropertyRefScalarExpressionRecursiveContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression '.' IDENTIFIER
            Contract.Requires(context.ChildCount == 3);

            SqlScalarExpression memberExpression = (SqlScalarExpression)this.Visit(context.children[0]);
            SqlIdentifier indentifier = SqlIdentifier.Create(context.children[2].GetText());

            return SqlPropertyRefScalarExpression.Create(memberExpression, indentifier);
        }

        public override SqlObject VisitSubqueryScalarExpression([NotNull] sqlParser.SubqueryScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // '(' sql_query ')'
            Contract.Requires(context.ChildCount == 3);

            SqlQuery subquery = (SqlQuery)this.Visit(context.children[1]);
            return SqlSubqueryScalarExpression.Create(subquery);
        }

        public override SqlObject VisitUnaryScalarExpression([NotNull] sqlParser.UnaryScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // unary_operator scalar_expression
            Contract.Requires(context.ChildCount == 2);

            string unaryOperatorText = context.children[0].GetText();
            if (!CstToAstVisitor.unaryOperatorKindLookup.TryGetValue(unaryOperatorText, out SqlUnaryScalarOperatorKind unaryOperator))
            {
                throw new ArgumentOutOfRangeException($"Unknown unary operator: {unaryOperatorText}.");
            }

            SqlScalarExpression expression = (SqlScalarExpression)this.Visit(context.children[1]);

            return SqlUnaryScalarExpression.Create(unaryOperator, expression);
        }

        private sealed class UnknownSqlObjectException : ArgumentOutOfRangeException
        {
            public UnknownSqlObjectException(SqlObject sqlObject, Exception innerException = null)
                : base(
                      message: $"Unknown {nameof(SqlObject)}: {sqlObject?.GetType()?.ToString() ?? "<NULL>"}",
                      innerException: innerException)
            {
            }
        }

        private static string GetStringValueFromNode(IParseTree parseTree)
        {
            string text = parseTree.GetText();
            string textWithoutQuotes = text.Substring(1, text.Length - 2);
            return textWithoutQuotes;
        }

        private static Number64 GetNumber64ValueFromNode(IParseTree parseTree)
        {
            Number64 number64;
            string text = parseTree.GetText();
            if (long.TryParse(text, out long longValue))
            {
                number64 = longValue;
            }
            else
            {
                number64 = double.Parse(text);
            }

            return number64;
        }
    }
}

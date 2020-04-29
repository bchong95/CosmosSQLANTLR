namespace CosmosSqlAntlr
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Sql;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    internal sealed class CstToAstVisitor : sqlBaseVisitor<SqlObject>
    {
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
                    sqlLiteralScalarExpression = SqlLiteralScalarExpression.Create(SqlStringLiteral.Create(terminalNode.GetText()));
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

        private sealed class UnknownSqlObjectException : ArgumentOutOfRangeException
        {
            public UnknownSqlObjectException(SqlObject sqlObject, Exception innerException = null)
                : base(
                      message: $"Unknown {nameof(SqlObject)}: {sqlObject?.GetType()?.ToString() ?? "<NULL>"}",
                      innerException: innerException)
            {
            }
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

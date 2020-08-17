﻿namespace CosmosSqlAntlr
{
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using CosmosSqlAntlr.Ast;
    using Microsoft.Build.Framework.XamlTypes;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Converts from ANTLR's CST to a CosmosDB SQL AST.
    /// The main difference is that a CST Context will contain every token like array start and delimiters, which is too verbose for most usecases.
    /// The children of a CST are also untyped (generic IParseTree), which doesn't give the contraints we need.
    /// In general the ANTLR parser could in theory generate an incorrect CST if the grammar file has a mistake,
    /// while and AST is strongly typed and any AST is a valid grammar (as long as the AST is defined correctly).
    /// </summary>
    public sealed class CstToAstVisitor : sqlBaseVisitor<SqlObject>
    {
        public static readonly CstToAstVisitor Singleton = new CstToAstVisitor();

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

        private CstToAstVisitor()
        {
        }

        public override SqlObject VisitProgram([NotNull] sqlParser.ProgramContext context)
        {
            Contract.Requires(context != null);

            return this.Visit(context.sql_query());
        }

        public override SqlObject VisitSql_query([NotNull] sqlParser.Sql_queryContext context)
        {
            Contract.Requires(context != null);

            SqlSelectClause sqlSelectClause = (SqlSelectClause)this.Visit(context.select_clause());

            SqlFromClause sqlFromClause;
            if (context.from_clause() != null)
            {
                sqlFromClause = (SqlFromClause)this.Visit(context.from_clause());
            }
            else
            {
                sqlFromClause = default;
            }

            SqlWhereClause sqlWhereClause;
            if (context.where_clause() != null)
            {
                sqlWhereClause = (SqlWhereClause)this.Visit(context.where_clause());
            }
            else
            {
                sqlWhereClause = default;
            }

            SqlOrderbyClause sqlOrderByClause;
            if (context.order_by_clause() != null)
            {
                sqlOrderByClause = (SqlOrderbyClause)this.Visit(context.order_by_clause());
            }
            else
            {
                sqlOrderByClause = default;
            }

            SqlGroupByClause sqlGroupByClause;
            if (context.group_by_clause() != default)
            {
                sqlGroupByClause = (SqlGroupByClause)this.Visit(context.group_by_clause());
            }
            else
            {
                sqlGroupByClause = default;
            }

            SqlOffsetLimitClause sqlOffsetLimitClause;
            if (context.offset_limit_clause() != default)
            {
                sqlOffsetLimitClause = (SqlOffsetLimitClause)this.Visit(context.offset_limit_clause());
            }
            else
            {
                sqlOffsetLimitClause = default;
            }

            return SqlQuery.Create(
                sqlSelectClause,
                sqlFromClause,
                sqlWhereClause,
                sqlGroupByClause,
                sqlOrderByClause,
                sqlOffsetLimitClause);
        }

        #region SELECT
        public override SqlObject VisitSelect_clause([NotNull] sqlParser.Select_clauseContext context)
        {
            SqlSelectSpec sqlSelectSpec = (SqlSelectSpec)this.Visit(context.selection());
            SqlTopSpec sqlTopSpec;
            if (context.top_spec() != default)
            {
                sqlTopSpec = (SqlTopSpec)this.Visit(context.top_spec());
            }
            else
            {
                sqlTopSpec = default;
            }

            bool distinct = context.K_DISTINCT() != default;

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

            SqlScalarExpression scalarExpression = (SqlScalarExpression)this.Visit(context.scalar_expression());
            SqlSelectValueSpec sqlSelectValueSpec = SqlSelectValueSpec.Create(scalarExpression);
            return sqlSelectValueSpec;
        }

        public override SqlObject VisitSelect_list_spec([NotNull] sqlParser.Select_list_specContext context)
        {
            Contract.Requires(context != null);

            List<SqlSelectItem> sqlSelectItems = new List<SqlSelectItem>();
            foreach (sqlParser.Select_itemContext selectItemContext in context.select_item())
            {
                SqlSelectItem selectItem = (SqlSelectItem)this.Visit(selectItemContext);
                sqlSelectItems.Add(selectItem);
            }

            return SqlSelectListSpec.Create(sqlSelectItems);
        }

        public override SqlObject VisitSelect_item([NotNull] sqlParser.Select_itemContext context)
        {
            Contract.Requires(context != null);

            SqlScalarExpression sqlScalarExpression = (SqlScalarExpression)this.Visit(context.scalar_expression());
            SqlIdentifier alias;
            if (context.IDENTIFIER() != null)
            {
                alias = SqlIdentifier.Create(context.IDENTIFIER().GetText());
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

            Number64 topCount = CstToAstVisitor.GetNumber64ValueFromNode(context.NUMERIC_LITERAL());
            return SqlTopSpec.Create(SqlNumberLiteral.Create(topCount));
        }
        #endregion
        #region FROM
        public override SqlObject VisitFrom_clause([NotNull] sqlParser.From_clauseContext context)
        {
            Contract.Requires(context != null);

            SqlCollectionExpression collectionExpression = (SqlCollectionExpression)this.Visit(context.collection_expression());

            return SqlFromClause.Create(collectionExpression);
        }

        public override SqlObject VisitAliasedCollectionExpression([NotNull] sqlParser.AliasedCollectionExpressionContext context)
        {
            Contract.Requires(context != null);

            SqlCollection sqlCollection = (SqlCollection)this.Visit(context.collection());
            SqlIdentifier alias;
            if (context.IDENTIFIER() != null)
            {
                alias = SqlIdentifier.Create(context.IDENTIFIER().GetText());
            }
            else
            {
                alias = default;
            }

            return SqlAliasedCollectionExpression.Create(sqlCollection, alias);
        }

        public override SqlObject VisitArrayIteratorCollectionExpression([NotNull] sqlParser.ArrayIteratorCollectionExpressionContext context)
        {
            Contract.Requires(context != null);

            SqlCollection sqlCollection = (SqlCollection)this.Visit(context.collection());
            SqlIdentifier identifier = SqlIdentifier.Create(context.IDENTIFIER().GetText());

            return SqlArrayIteratorCollectionExpression.Create(identifier, sqlCollection);
        }

        public override SqlObject VisitJoinCollectionExpression([NotNull] sqlParser.JoinCollectionExpressionContext context)
        {
            Contract.Requires(context != null);

            SqlCollectionExpression left = (SqlCollectionExpression)this.Visit(context.collection_expression(0));
            SqlCollectionExpression right = (SqlCollectionExpression)this.Visit(context.collection_expression(1));

            return SqlJoinCollectionExpression.Create(left, right);
        }

        public override SqlObject VisitInputPathCollection([NotNull] sqlParser.InputPathCollectionContext context)
        {
            Contract.Requires(context != null);

            SqlIdentifier identifier = SqlIdentifier.Create(context.IDENTIFIER().GetText());
            SqlPathExpression pathExpression;
            if (context.path_expression() != null)
            {
                pathExpression = (SqlPathExpression)this.Visit(context.path_expression());
            }
            else
            {
                pathExpression = null;
            }

            return SqlInputPathCollection.Create(identifier, pathExpression);
        }

        public override SqlObject VisitSubqueryCollection([NotNull] sqlParser.SubqueryCollectionContext context)
        {
            Contract.Requires(context != null);

            SqlQuery subQuery = (SqlQuery)this.Visit(context.sql_query());

            return SqlSubqueryCollection.Create(subQuery);
        }

        public override SqlObject VisitEpsilonPathExpression([NotNull] sqlParser.EpsilonPathExpressionContext context)
        {
            return null;
        }

        public override SqlObject VisitIdentifierPathExpression([NotNull] sqlParser.IdentifierPathExpressionContext context)
        {
            Contract.Requires(context != null);

            SqlPathExpression pathExpression = (SqlPathExpression)this.Visit(context.path_expression());
            SqlIdentifier identifier = SqlIdentifier.Create(context.IDENTIFIER().GetText());

            return SqlIdentifierPathExpression.Create(parentPath: pathExpression, value: identifier);
        }

        public override SqlObject VisitNumberPathExpression([NotNull] sqlParser.NumberPathExpressionContext context)
        {
            Contract.Requires(context != null);

            SqlPathExpression pathExpression = (SqlPathExpression)this.Visit(context.path_expression());
            SqlNumberLiteral number = SqlNumberLiteral.Create(CstToAstVisitor.GetNumber64ValueFromNode(context.NUMERIC_LITERAL()));

            return SqlNumberPathExpression.Create(pathExpression, number);
        }

        public override SqlObject VisitStringPathExpression([NotNull] sqlParser.StringPathExpressionContext context)
        {
            Contract.Requires(context != null);

            SqlPathExpression pathExpression = (SqlPathExpression)this.Visit(context.path_expression());
            SqlStringLiteral stringIndex = SqlStringLiteral.Create(CstToAstVisitor.GetStringValueFromNode(context.STRING_LITERAL()));

            return SqlStringPathExpression.Create(pathExpression, stringIndex);
        }
        #endregion
        #region WHERE
        public override SqlObject VisitWhere_clause([NotNull] sqlParser.Where_clauseContext context)
        {
            Contract.Requires(context != null);
            SqlScalarExpression sqlScalarExpression = (SqlScalarExpression)this.Visit(context.scalar_expression());
            return SqlWhereClause.Create(sqlScalarExpression);
        }
        #endregion
        #region GROUP BY
        public override SqlObject VisitGroup_by_clause([NotNull] sqlParser.Group_by_clauseContext context)
        {
            Contract.Requires(context != null);

            List<SqlScalarExpression> groupByColumns = new List<SqlScalarExpression>();
            foreach (sqlParser.Scalar_expressionContext scalarExpressionContext in context.scalar_expression_list().scalar_expression())
            {
                groupByColumns.Add((SqlScalarExpression)this.Visit(scalarExpressionContext));
            }

            return SqlGroupByClause.Create(groupByColumns);
        }
        #endregion
        #region ORDER BY
        public override SqlObject VisitOrder_by_clause([NotNull] sqlParser.Order_by_clauseContext context)
        {
            Contract.Requires(context != null);

            List<SqlOrderByItem> orderByItems = new List<SqlOrderByItem>();
            foreach (sqlParser.Order_by_itemContext orderByItemContext in context.order_by_items().order_by_item())
            {
                SqlScalarExpression expression = (SqlScalarExpression)this.Visit(orderByItemContext.scalar_expression());
                bool isDescending = false;
                if (orderByItemContext.sort_order() != null)
                {
                    if (orderByItemContext.sort_order().K_ASC() != null)
                    {
                        isDescending = false;
                    }
                    else if (orderByItemContext.sort_order().K_DESC() != null)
                    {
                        isDescending = true;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException($"Unknown sort order : {orderByItemContext.sort_order()}.");
                    }
                }

                SqlOrderByItem orderByItem = SqlOrderByItem.Create(expression, isDescending);
                orderByItems.Add(orderByItem);
            }

            return SqlOrderbyClause.Create(orderByItems);
        }
        #endregion
        #region OFFSET LIMIT
        public override SqlObject VisitOffset_limit_clause([NotNull] sqlParser.Offset_limit_clauseContext context)
        {
            Contract.Requires(context != null);

            SqlOffsetSpec sqlOffsetSpec = SqlOffsetSpec.Create(
                SqlNumberLiteral.Create(
                    CstToAstVisitor.GetNumber64ValueFromNode(
                        context.offset_count().NUMERIC_LITERAL())));

            SqlLimitSpec sqlLimitSpec = SqlLimitSpec.Create(
                SqlNumberLiteral.Create(
                    CstToAstVisitor.GetNumber64ValueFromNode(
                        context.limit_count().NUMERIC_LITERAL())));

            return SqlOffsetLimitClause.Create(sqlOffsetSpec, sqlLimitSpec);
        }
        #endregion
        #region ScalarExpressions

        public override SqlObject VisitArrayCreateScalarExpression([NotNull] sqlParser.ArrayCreateScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.ChildCount >= 2); // start array and end array tokens

            List<SqlScalarExpression> arrayItems = new List<SqlScalarExpression>();
            if (context.scalar_expression_list() != null)
            {
                foreach (sqlParser.Scalar_expressionContext scalarExpressionContext in context.scalar_expression_list().scalar_expression())
                {
                    arrayItems.Add((SqlScalarExpression)this.Visit(scalarExpressionContext));
                }
            }

            return SqlArrayCreateScalarExpression.Create(arrayItems);
        }

        public override SqlObject VisitArrayScalarExpression([NotNull] sqlParser.ArrayScalarExpressionContext context)
        {
            Contract.Requires(context != null);

            SqlQuery sqlQuery = (SqlQuery)this.Visit(context.sql_query());
            return SqlArrayScalarExpression.Create(sqlQuery);
        }

        public override SqlObject VisitBetweenScalarExpression([NotNull] sqlParser.BetweenScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression K_NOT? K_BETWEEN scalar_expression K_AND scalar_expression

            SqlScalarExpression needle = (SqlScalarExpression)this.Visit(context.binary_scalar_expression(0));
            bool not = context.K_NOT() != null;
            SqlScalarExpression start = (SqlScalarExpression)this.Visit(context.binary_scalar_expression(1));
            SqlScalarExpression end = (SqlScalarExpression)this.Visit(context.binary_scalar_expression(2));

            return SqlBetweenScalarExpression.Create(needle, start, end, not);
        }

        public override SqlObject VisitBinary_scalar_expression([NotNull] sqlParser.Binary_scalar_expressionContext context)
        {
            Contract.Requires(context != null);

            SqlObject sqlObject;
            if (context.unary_scalar_expression() != null)
            {
                sqlObject = (SqlScalarExpression)this.Visit(context.unary_scalar_expression());
            }
            else
            {
                // scalar_expression binary_operator scalar_expression
                Contract.Requires(context.ChildCount == 3);

                SqlScalarExpression left = (SqlScalarExpression)this.Visit(context.binary_scalar_expression(0));
                if (!CstToAstVisitor.binaryOperatorKindLookup.TryGetValue(
                    context.children[1].GetText(),
                    out SqlBinaryScalarOperatorKind operatorKind))
                {
                    throw new ArgumentOutOfRangeException($"Unknown binary operator: {context.children[1].GetText()}.");
                }

                SqlScalarExpression right = (SqlScalarExpression)this.Visit(context.binary_scalar_expression(1));

                sqlObject = SqlBinaryScalarExpression.Create(operatorKind, left, right);
            }

            return sqlObject;
        }

        public override SqlObject VisitCoalesceScalarExpression([NotNull] sqlParser.CoalesceScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression '??' scalar_expression

            SqlScalarExpression left = (SqlScalarExpression)this.Visit(context.scalar_expression(0));
            SqlScalarExpression right = (SqlScalarExpression)this.Visit(context.scalar_expression(1));

            return SqlCoalesceScalarExpression.Create(left, right);
        }

        public override SqlObject VisitConditionalScalarExpression([NotNull] sqlParser.ConditionalScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression '?' scalar_expression ':' scalar_expression
            Contract.Requires(context.ChildCount == 5);

            SqlScalarExpression condition = (SqlScalarExpression)this.Visit(context.scalar_expression(0));
            SqlScalarExpression consequent = (SqlScalarExpression)this.Visit(context.scalar_expression(1));
            SqlScalarExpression alternative = (SqlScalarExpression)this.Visit(context.scalar_expression(2));
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
            // (K_UDF '.')? IDENTIFIER '(' scalar_expression_list? ')'

            bool udf = context.K_UDF() != null;
            SqlIdentifier identifier = SqlIdentifier.Create(context.IDENTIFIER().GetText());
            List<SqlScalarExpression> arguments = new List<SqlScalarExpression>();
            if (context.scalar_expression_list() != null)
            {
                foreach (sqlParser.Scalar_expressionContext scalarExpressionContext in context.scalar_expression_list().scalar_expression())
                {
                    arguments.Add((SqlScalarExpression)this.Visit(scalarExpressionContext));
                }
            }

            return SqlFunctionCallScalarExpression.Create(identifier, udf, arguments);
        }

        public override SqlObject VisitIn_scalar_expression([NotNull] sqlParser.In_scalar_expressionContext context)
        {
            Contract.Requires(context != null);
            // scalar_expression K_NOT? K_IN '(' scalar_expression_list ')'

            SqlScalarExpression needle = (SqlScalarExpression)this.Visit(context.binary_scalar_expression());
            bool not = context.K_NOT() != null;
            List<SqlScalarExpression> searchList = new List<SqlScalarExpression>();
            foreach (sqlParser.Scalar_expressionContext scalarExpressionContext in context.scalar_expression_list().scalar_expression())
            {
                searchList.Add((SqlScalarExpression)this.Visit(scalarExpressionContext));
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
            // primary_expression '[' scalar_expression ']'

            SqlScalarExpression memberExpression = (SqlScalarExpression)this.Visit(context.primary_expression());
            SqlScalarExpression indexExpression = (SqlScalarExpression)this.Visit(context.scalar_expression());

            return SqlMemberIndexerScalarExpression.Create(memberExpression, indexExpression);
        }

        public override SqlObject VisitObjectCreateScalarExpression([NotNull] sqlParser.ObjectCreateScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // '{' object_propertty_list? '}'

            List<SqlObjectProperty> properties = new List<SqlObjectProperty>();
            if (context.object_propertty_list() != null)
            {
                sqlParser.Object_propertyContext[] propertyContexts = context.object_propertty_list().object_property();
                foreach (sqlParser.Object_propertyContext objectPropertyContext in propertyContexts)
                {
                    string name = CstToAstVisitor.GetStringValueFromNode(objectPropertyContext.STRING_LITERAL());
                    SqlScalarExpression value = (SqlScalarExpression)this.Visit(objectPropertyContext.scalar_expression());

                    SqlObjectProperty property = SqlObjectProperty.Create(
                        SqlPropertyName.Create(name),
                        value);
                    properties.Add(property);
                }
            }

            return SqlObjectCreateScalarExpression.Create(properties);
        }

        public override SqlObject VisitPropertyRefScalarExpressionBase([NotNull] sqlParser.PropertyRefScalarExpressionBaseContext context)
        {
            Contract.Requires(context != null);
            // IDENTIFIER

            return SqlPropertyRefScalarExpression.Create(
                memberExpression: null,
                SqlIdentifier.Create(context.IDENTIFIER().GetText()));
        }

        public override SqlObject VisitPropertyRefScalarExpressionRecursive([NotNull] sqlParser.PropertyRefScalarExpressionRecursiveContext context)
        {
            Contract.Requires(context != null);
            // primary_expression '.' IDENTIFIER

            SqlScalarExpression memberExpression = (SqlScalarExpression)this.Visit(context.primary_expression());
            SqlIdentifier indentifier = SqlIdentifier.Create(context.IDENTIFIER().GetText());

            return SqlPropertyRefScalarExpression.Create(memberExpression, indentifier);
        }

        public override SqlObject VisitSubqueryScalarExpression([NotNull] sqlParser.SubqueryScalarExpressionContext context)
        {
            Contract.Requires(context != null);
            // '(' sql_query ')'

            SqlQuery subquery = (SqlQuery)this.Visit(context.sql_query());
            return SqlSubqueryScalarExpression.Create(subquery);
        }

        public override SqlObject VisitUnary_scalar_expression([NotNull] sqlParser.Unary_scalar_expressionContext context)
        {
            Contract.Requires(context != null);
            if (context.primary_expression() != null)
            {
                return this.Visit(context.primary_expression());
            }

            string unaryOperatorText = context.unary_operator().GetText();
            if (!CstToAstVisitor.unaryOperatorKindLookup.TryGetValue(
                unaryOperatorText,
                out SqlUnaryScalarOperatorKind unaryOperator))
            {
                throw new ArgumentOutOfRangeException($"Unknown unary operator: {unaryOperatorText}.");
            }

            SqlScalarExpression expression = (SqlScalarExpression)this.Visit(context.unary_scalar_expression());

            return SqlUnaryScalarExpression.Create(unaryOperator, expression);
        }
        #endregion
        #region ArtificalScalarExpressions
        public override SqlObject VisitParenthesizedScalarExperession([NotNull] sqlParser.ParenthesizedScalarExperessionContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.ChildCount == 3); // open parens and close parens

            IParseTree parseTree = context.children[1];
            return this.Visit(parseTree);
        }

        public override SqlObject VisitLogical_scalar_expression([NotNull] sqlParser.Logical_scalar_expressionContext context)
        {
            Contract.Requires(context != null);

            SqlObject sqlObject;
            if (context.binary_scalar_expression() != null)
            {
                sqlObject = this.Visit(context.binary_scalar_expression());
            }
            else if (context.in_scalar_expression() != null)
            {
                sqlObject = this.Visit(context.in_scalar_expression());
            }
            else
            {
                throw new NotImplementedException();
            }

            return sqlObject;
        }
        #endregion
        #region NOT IMPLEMENTED ON PURPOSE
        public override SqlObject VisitLimit_count([NotNull] sqlParser.Limit_countContext context)
        {
            throw new NotSupportedException();
        }

        public override SqlObject VisitLiteral([NotNull] sqlParser.LiteralContext context)
        {
            throw new NotSupportedException();
        }

        public override SqlObject VisitObject_propertty_list([NotNull] sqlParser.Object_propertty_listContext context)
        {
            throw new NotSupportedException();
        }

        public override SqlObject VisitObject_property([NotNull] sqlParser.Object_propertyContext context)
        {
            throw new NotSupportedException();
        }

        public override SqlObject VisitOffset_count([NotNull] sqlParser.Offset_countContext context)
        {
            throw new NotSupportedException();
        }

        public override SqlObject VisitOrder_by_item([NotNull] sqlParser.Order_by_itemContext context)
        {
            throw new NotSupportedException();
        }

        public override SqlObject VisitOrder_by_items([NotNull] sqlParser.Order_by_itemsContext context)
        {
            throw new NotSupportedException();
        }

        public override SqlObject VisitSort_order([NotNull] sqlParser.Sort_orderContext context)
        {
            throw new NotSupportedException();
        }

        public override SqlObject VisitScalar_expression_list([NotNull] sqlParser.Scalar_expression_listContext context)
        {
            throw new NotSupportedException();
        }
        #endregion

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

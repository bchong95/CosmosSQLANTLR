// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------
namespace CosmosSqlAntlr.Ast
{
    using System;
	using CosmosSqlAntlr.Ast.Visitors;
    using System.Collections.Generic;

    public sealed class SqlGroupByClause : SqlObject
    {
        private SqlGroupByClause(IReadOnlyList<SqlScalarExpression> expressions)
            : base(SqlObjectKind.GroupByClause)
        {
            if (expressions == null)
            {
                throw new ArgumentNullException($"{nameof(expressions)}");
            }

            foreach (SqlScalarExpression expression in expressions)
            {
                if (expression == null)
                {
                    throw new ArgumentException($"{nameof(expressions)} must not have null items.");
                }
            }

            this.Expressions = expressions;
        }

        public IReadOnlyList<SqlScalarExpression> Expressions
        {
            get;
        }

        public static SqlGroupByClause Create(params SqlScalarExpression[] expressions)
        {
            return new SqlGroupByClause(expressions);
        }

        public static SqlGroupByClause Create(IReadOnlyList<SqlScalarExpression> expressions)
        {
            return new SqlGroupByClause(expressions);
        }

        public override void Accept(SqlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override TResult Accept<TResult>(SqlObjectVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public override TResult Accept<T, TResult>(SqlObjectVisitor<T, TResult> visitor, T input)
        {
            return visitor.Visit(this, input);
        }
    }
}

//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace CosmosSqlAntlr.Ast
{
    using System;
	using CosmosSqlAntlr.Ast.Visitors;

    public sealed class SqlWhereClause : SqlObject
    {
        private SqlWhereClause(SqlScalarExpression filterExpression)
            : base(SqlObjectKind.WhereClause)
        {
            if (filterExpression == null)
            {
                throw new ArgumentNullException("filterExpression");
            }

            this.FilterExpression = filterExpression;
        }

        public SqlScalarExpression FilterExpression
        {
            get;
        }

        public static SqlWhereClause Create(SqlScalarExpression filterExpression)
        {
            return new SqlWhereClause(filterExpression);
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

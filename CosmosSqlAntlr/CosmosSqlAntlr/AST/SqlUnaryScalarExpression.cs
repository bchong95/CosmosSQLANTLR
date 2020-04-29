﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Cosmos.Sql
{
    using System;

    internal sealed class SqlUnaryScalarExpression : SqlScalarExpression
    {
        private SqlUnaryScalarExpression(
            SqlUnaryScalarOperatorKind operatorKind,
            SqlScalarExpression expression)
            : base(SqlObjectKind.UnaryScalarExpression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this.OperatorKind = operatorKind;
            this.Expression = expression;
        }

        public SqlUnaryScalarOperatorKind OperatorKind
        {
            get;
        }

        public SqlScalarExpression Expression
        {
            get;
        }

        public static SqlUnaryScalarExpression Create(
            SqlUnaryScalarOperatorKind operatorKind,
            SqlScalarExpression expression)
        {
            return new SqlUnaryScalarExpression(operatorKind, expression);
        }

        public override void Accept(SqlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override TResult Accept<TResult>(SqlObjectVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Accept(SqlScalarExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override TResult Accept<TResult>(SqlScalarExpressionVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public override TResult Accept<T, TResult>(SqlObjectVisitor<T, TResult> visitor, T input)
        {
            return visitor.Visit(this, input);
        }

        public override TResult Accept<T, TResult>(SqlScalarExpressionVisitor<T, TResult> visitor, T input)
        {
            return visitor.Visit(this, input);
        }
    }
}

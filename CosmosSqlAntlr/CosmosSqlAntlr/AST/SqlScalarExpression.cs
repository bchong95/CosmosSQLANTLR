﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace CosmosSqlAntlr.Ast
{
    using CosmosSqlAntlr.Ast.Visitors;

    public abstract class SqlScalarExpression : SqlObject
    {
        protected SqlScalarExpression(SqlObjectKind kind)
            : base(kind)
        {
        }

        public abstract void Accept(SqlScalarExpressionVisitor visitor);

        public abstract TResult Accept<TResult>(SqlScalarExpressionVisitor<TResult> visitor);

        public abstract TResult Accept<T, TResult>(SqlScalarExpressionVisitor<T, TResult> visitor, T input);
    }
}

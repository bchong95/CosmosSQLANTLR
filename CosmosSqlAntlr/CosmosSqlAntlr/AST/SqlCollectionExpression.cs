﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace CosmosSqlAntlr.Ast
{
    using CosmosSqlAntlr.Ast.Visitors;

    public abstract class SqlCollectionExpression : SqlObject
    {
        protected SqlCollectionExpression(SqlObjectKind kind)
            : base(kind)
        {
        }

        public abstract void Accept(SqlCollectionExpressionVisitor visitor);

        public abstract TResult Accept<TResult>(SqlCollectionExpressionVisitor<TResult> visitor);

        public abstract TResult Accept<T, TResult>(SqlCollectionExpressionVisitor<T, TResult> visitor, T input);
    }
}

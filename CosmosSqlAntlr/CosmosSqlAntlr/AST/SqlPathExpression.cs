﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace CosmosSqlAntlr.Ast
{
    using CosmosSqlAntlr.Ast.Visitors;

    public abstract class SqlPathExpression : SqlObject
    {
        protected SqlPathExpression(SqlObjectKind kind, SqlPathExpression parentPath)
            : base(kind)
        {
            this.ParentPath = parentPath;
        }

        public SqlPathExpression ParentPath
        {
            get;
        }

        public abstract void Accept(SqlPathExpressionVisitor visitor);

        public abstract TResult Accept<TResult>(SqlPathExpressionVisitor<TResult> visitor);
    }
}

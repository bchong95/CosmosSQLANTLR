﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace CosmosSqlAntlr.Ast.Visitors
{
    public abstract class SqlCollectionVisitor
    {
        public abstract void Visit(SqlInputPathCollection collection);

        public abstract void Visit(SqlSubqueryCollection collection);
    }

    public abstract class SqlCollectionVisitor<TResult>
    {
        public abstract TResult Visit(SqlInputPathCollection collection);

        public abstract TResult Visit(SqlSubqueryCollection collection);
    }

    public abstract class SqlCollectionVisitor<TArg, TOuput>
    {
        public abstract TOuput Visit(SqlInputPathCollection collection, TArg input);

        public abstract TOuput Visit(SqlSubqueryCollection collection, TArg input);
    }
}

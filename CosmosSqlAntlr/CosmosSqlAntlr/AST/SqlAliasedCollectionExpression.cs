﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Cosmos.Sql
{
    using System;

    // This class represents a collection expression that is comprised of a collection definition and an 
    // optional alias.
    // Examples:
    //  FROM Person p
    //  FROM [1, 3, 5, 7] a

    internal sealed class SqlAliasedCollectionExpression : SqlCollectionExpression
    {
        private SqlAliasedCollectionExpression(
            SqlCollection collection,
            SqlIdentifier alias)
            : base(SqlObjectKind.AliasedCollectionExpression)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            this.Collection = collection;
            this.Alias = alias;
        }

        public SqlCollection Collection
        {
            get;
        }

        public SqlIdentifier Alias
        {
            get;
        }

        public static SqlAliasedCollectionExpression Create(
            SqlCollection collection,
            SqlIdentifier alias)
        {
            return new SqlAliasedCollectionExpression(collection, alias);
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

        public override void Accept(SqlCollectionExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override TResult Accept<TResult>(SqlCollectionExpressionVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public override TResult Accept<T, TResult>(SqlCollectionExpressionVisitor<T, TResult> visitor, T input)
        {
            return visitor.Visit(this, input);
        }
    }
}

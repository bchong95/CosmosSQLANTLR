﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Cosmos.Sql
{
    using System;

    internal sealed class SqlInputPathCollection : SqlCollection
    {
        private SqlInputPathCollection(
            SqlIdentifier input,
            SqlPathExpression relativePath)
            : base(SqlObjectKind.InputPathCollection)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            this.Input = input;
            this.RelativePath = relativePath;
        }

        public SqlIdentifier Input
        {
            get;
        }

        public SqlPathExpression RelativePath
        {
            get;
        }

        public static SqlInputPathCollection Create(
            SqlIdentifier input,
            SqlPathExpression relativePath)
        {
            return new SqlInputPathCollection(input, relativePath);
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

        public override void Accept(SqlCollectionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override TResult Accept<TResult>(SqlCollectionVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public override TResult Accept<T, TResult>(SqlCollectionVisitor<T, TResult> visitor, T input)
        {
            return visitor.Visit(this, input);
        }
    }
}

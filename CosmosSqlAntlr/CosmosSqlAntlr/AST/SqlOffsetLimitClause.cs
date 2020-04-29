﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Cosmos.Sql
{
    using System;

    internal sealed class SqlOffsetLimitClause : SqlObject
    {
        private SqlOffsetLimitClause(SqlOffsetSpec offsetSpec, SqlLimitSpec limitSpec)
            : base(SqlObjectKind.OffsetLimitClause)
        {
            if (offsetSpec == null)
            {
                throw new ArgumentNullException($"{nameof(offsetSpec)}");
            }

            if (limitSpec == null)
            {
                throw new ArgumentNullException($"{nameof(limitSpec)}");
            }

            this.OffsetSpec = offsetSpec;
            this.LimitSpec = limitSpec;
        }

        public SqlOffsetSpec OffsetSpec
        {
            get;
        }

        public SqlLimitSpec LimitSpec
        {
            get;
        }

        public static SqlOffsetLimitClause Create(SqlOffsetSpec offsetSpec, SqlLimitSpec limitSpec)
        {
            return new SqlOffsetLimitClause(offsetSpec, limitSpec);
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

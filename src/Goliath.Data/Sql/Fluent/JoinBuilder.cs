﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    using Mapping;

    class JoinBuilder : IQueryBuilder, IJoinable, IJoinOperation
    {
        QueryBuilder builder;

        public string JoinTableName { get; internal set; }
        public string JoinTableAlias { get; internal set; }
        public JoinType Type { get; set; }

        public string JoinLeftColumn { get; internal set; }
        public string JoinLeftTableAlias { get; internal set; }
        public string JoinRightColumn { get; internal set; }
        public string TableName { get; internal set; }

        public ComparisonOperator JoinOperator { get; private set; }

        public JoinBuilder(QueryBuilder builder, string tablename, string joinTableName, string joinTableAlias)
        {
            TableName = tablename;
            JoinTableAlias = joinTableAlias;
            JoinTableName = joinTableName;
            this.builder = builder;
        }

        #region IQueryBuilder Members

        public IJoinable InnerJoin(string tableName, string alias)
        {
            return builder.InnerJoin(tableName, alias);
        }

        public IJoinable LeftJoin(string tableName, string alias)
        {
            return builder.LeftJoin(tableName, alias);
        }

        public IJoinable RightJoin(string tableName, string alias)
        {
            return builder.RightJoin(tableName, alias);
        }

        public IFilterClause Where(string propertyName)
        {
            return builder.Where(propertyName);
        }

        public IFilterClause Where(string tableAlias, string propertyName)
        {
            return builder.Where(tableAlias, propertyName);
        }

        public ISorterClause OrderBy(string columnName)
        {
            return builder.OrderBy(columnName);
        }

        public ISorterClause OrderBy(string tableAlias, string columnName)
        {
            return builder.OrderBy(tableAlias, columnName);
        }

        #endregion

        #region IQueryFetchable Members

        public IFetchable Limit(int i)
        {
            return builder.Limit(i);
        }

        public IFetchable Offset(int i)
        {
            return builder.Offset(i);
        }

        #endregion

        #region IFetchable Members

        public System.Collections.Generic.ICollection<T> FetchAll<T>()
        {
            return builder.FetchAll<T>();
        }

        public T FetchOne<T>()
        {
            return builder.FetchOne<T>();
        }

        #endregion

        #region IJoinable Members

        public IJoinOperation On(string tableAlias, string propertyName)
        {
            JoinLeftColumn = propertyName;
            JoinLeftTableAlias = tableAlias;
            return this;
        }

        #endregion

        #region IJoinOperation Members

        public IQueryBuilder EqualTo(string propertyName)
        {
            JoinRightColumn = propertyName;
            return this;
        }

        #endregion
    }
}

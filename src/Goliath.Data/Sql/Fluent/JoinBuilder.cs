using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    using Mapping;

    class JoinBuilder : IJoinQueryBuilder, IJoinable, IJoinOperation
    {
        QueryBuilder builder;

        public string JoinTableName { get; private set; }
        public string JoinTableAlias { get; private set; }
        public JoinType Type { get; set; }

        public string JoinLeftColumn { get; private set; }
        public string JoinRightColumn { get; private set; }
        public string TableName { get; private set; }

        public ComparisonOperator JoinOperator { get; private set; }

        public JoinBuilder(QueryBuilder builder, string tablename, string joinTableName, string joinTableAlias)
        {
            TableName = tablename;
            JoinTableAlias = joinTableAlias;
            JoinTableName = joinTableName;
            this.builder = builder;
        }

        #region IJoinQueryBuilder Members

        public IWhereOnRelation ForJoin(string alias)
        {
            return builder.ForJoin(alias);
        }

        #endregion

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

        public System.Collections.ICollection FetchAll()
        {
            return builder.FetchAll();
        }

        public object FetchOne()
        {
            return builder.FetchOne();
        }

        #endregion

        #region IJoinable Members

        public IJoinOperation On(string propertyName)
        {
            JoinLeftColumn = propertyName;
            return this;
        }

        #endregion

        #region IJoinOperation Members

        public IJoinQueryBuilder EqualTo(string propertyName)
        {
            JoinRightColumn = propertyName;
            return this;
        }

        #endregion
    }
}

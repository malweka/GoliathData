using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    partial class QueryBuilder : IJoinable
    {
        public IJoinable InnerJoin(string tableName, string alias)
        {
            var join = BuildJoin(tableName, alias, JoinType.Inner);
            joins.Add(alias, join);
            return join;
        }

        public IJoinable LeftJoin(string tableName, string alias)
        {
            var join = BuildJoin(tableName, alias, JoinType.Left);
            joins.Add(alias, join);
            return join;
        }

        public IJoinable RightJoin(string tableName, string alias)
        {
            var join = BuildJoin(tableName, alias, JoinType.Right);
            joins.Add(alias, join);
            return join;
        }

        JoinBuilder BuildJoin(string joinTableName, string jointTableAlias, JoinType type)
        {
            JoinBuilder join = new JoinBuilder(this, this.tableName, joinTableName, jointTableAlias) { Type = type };
            return join;
        }


        #region IJoinable Members

        IJoinOperation IJoinable.On(string propertyName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

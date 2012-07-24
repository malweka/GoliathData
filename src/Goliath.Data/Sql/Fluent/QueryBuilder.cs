using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    using Providers;
    using Mapping;
    using DataAccess;

    partial class QueryBuilder : IQueryBuilder, ITableNameBuilder
    {
        List<string> columnNames = new List<string>();
        List<QueryParam> parameters = new List<QueryParam>();
        Dictionary<string, JoinBuilder> joins = new Dictionary<string, JoinBuilder>();
        SqlSelectColumnFormatter columnFormatter = new SqlSelectColumnFormatter();
        MapConfig mapping;
        SqlDialect dialect;
        string tableName;
        string alias;
        int limit;
        int offset;
        ISession session;

        public SqlSelectColumnFormatter ColumnFormatter
        {
            get { return columnFormatter; }
            set
            {
                if (value != null)
                    columnFormatter = value;
            }
        }

        public List<QueryParam> Parameters
        {
            get { return parameters; }
        }

        EntityMap Table { get; set; }


        public QueryBuilder(ISession session, List<string> columnNames)
        {
            if (columnNames != null)
                this.columnNames = columnNames;

            if (session == null)
                throw new ArgumentNullException("session");

            this.session = session;
            dialect = session.SessionFactory.DataSerializer.SqlDialect;
            this.mapping = session.SessionFactory.DbSettings.Map;

        }

        #region ITableNameBuilder Members

        public IQueryBuilder From(string tableName)
        {
            return From(tableName, null);
        }

        public IQueryBuilder From(string tableName, string alias)
        {
            this.tableName = tableName;
            this.alias = alias;
            return this;
        }

        #endregion

        #region IQueryFetchable Members

        public IFetchable Limit(int i)
        {
            this.limit = i;
            return this;
        }

        public IFetchable Offset(int i)
        {
            this.offset = i;
            return this;
        }

        #endregion

        #region IFetchable Members

        public ICollection<T> FetchAll<T>()
        {
            throw new NotImplementedException();
        }

        public T FetchOne<T>()
        {
            Type instanceType = typeof(T);
            
            throw new NotImplementedException();
        }

        #endregion

        void AddToParameterList(QueryParam param)
        {
            if (param != null)
            {
                Parameters.Add(param);
            }
        }
        internal string BuildSql()
        {
            StringBuilder sqlBuilder = new StringBuilder("SELECT ");

            if (string.IsNullOrEmpty(alias))
                alias = tableName;

            if (columnNames.Count < 1)
                sqlBuilder.Append("* ");
            else
                sqlBuilder.Append(ColumnFormatter.Format(columnNames, alias));

            sqlBuilder.AppendFormat(" FROM {0} {1} ", tableName, alias);

            if (joins.Count > 0)
            {
                string jtype = "JOIN";

                foreach (var join in joins.Values)
                {
                    switch (join.Type)
                    {
                        case JoinType.Inner:
                            jtype = "INNER JOIN";
                            break;
                        case JoinType.Left:
                            jtype = "LEFT JOIN";
                            break;
                        case JoinType.Right:
                            jtype = "RIGHT JOIN";
                            break;
                        case JoinType.Full:
                            jtype = "FULL JOIN";
                            break;
                    }

                    sqlBuilder.AppendFormat("{0} {1} {2} ON ", jtype, join.JoinTableName, join.JoinTableAlias);
                    sqlBuilder.AppendFormat("{0}.{1} = {2}.{3} ", alias, join.JoinRightColumn, join.JoinTableAlias, join.JoinLeftColumn);
                }
            }

            if (whereClauses.Count > 0)
            {
                var firstWhere = whereClauses[0];
                var sql = firstWhere.BuildSqlString(dialect, 0);
                AddToParameterList(sql.Item2);
                sqlBuilder.AppendFormat("WHERE {0} ", sql.Item1);

                if (whereClauses.Count > 1)
                {
                    for (int i = 1; i < whereClauses.Count; i++)
                    {
                        var where = whereClauses[i].BuildSqlString(dialect, i);
                        AddToParameterList(where.Item2);

                        string prep = "AND";
                        if (whereClauses[i].PreOperator != SqlOperator.AND)
                            prep = "OR";

                        sqlBuilder.AppendFormat("{0} {1} ", prep, where.Item1);
                    }
                }
            }

            if (sortClauses.Count > 0)
            {
                sqlBuilder.Append("ORDER BY ");
                sqlBuilder.Append(string.Join(", ", sortClauses));
            }

            return sqlBuilder.ToString();
        }
    }
}

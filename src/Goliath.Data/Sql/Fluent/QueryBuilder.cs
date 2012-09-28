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
        int limit=-1;
        int offset=-1;
        ISession session;

        internal  Dictionary<string, JoinBuilder> Joins
        {
            get { return joins; }
        }

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

        ICollection<T> RunQueryAndHydrateWithPaging<T>()
        {
            if (limit < 1)
                limit = 0;

            if (offset < 1)
                offset = 0;

            SqlCommandRunner runner = new SqlCommandRunner();
            var query = BuildSql();
            return runner.RunList<T>(session, query, limit, offset, Parameters.ToArray());
        }

        ICollection<T> RunQueryAndHydrate<T>()
        {
            SqlCommandRunner runner = new SqlCommandRunner();
            var query = BuildSql();
            return runner.RunList<T>(session, query, Parameters.ToArray());
        }

        #region IFetchable Members

        public ICollection<T> FetchAll<T>()
        {
            if ((limit > 0) || (offset > 0))
                return RunQueryAndHydrateWithPaging<T>();
            else
                return RunQueryAndHydrate<T>();
        }

        public T FetchOne<T>()
        {
            SqlCommandRunner runner = new SqlCommandRunner();
            var query = BuildSql();
            return runner.Run<T>(session, query, Parameters.ToArray());
        }

        #endregion

        void AddToParameterList(QueryParam param)
        {
            if (param != null)
            {
                Parameters.Add(param);
            }
        }

        public SqlQueryBody BuildSql()
        {
            SqlQueryBody queryBody = new SqlQueryBody();

            if (string.IsNullOrEmpty(alias))
            {
                alias = tableName;
                queryBody.From = tableName;
            }
            else
            {
                queryBody.From = string.Format("{0} {1}", tableName, alias);
            }

            if (columnNames.Count < 1)
                queryBody.ColumnEnumeration = "*";
            else
                 queryBody.ColumnEnumeration = ColumnFormatter.Format(columnNames, alias);

            StringBuilder joinBuilder = new StringBuilder();
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

                    joinBuilder.AppendFormat("{0} {1} {2} ON ", jtype, join.JoinTableName, join.JoinTableAlias);
                    joinBuilder.AppendFormat("{0}.{1} = {2}.{3} ", alias, join.JoinRightColumn, join.JoinTableAlias, join.JoinLeftColumn);
                }

                queryBody.JoinEnumeration = joinBuilder.ToString().Trim();
            }

            if (whereClauses.Count > 0)
            {
                var firstWhere = whereClauses[0];
                var sql = firstWhere.BuildSqlString(dialect, 0);
                AddToParameterList(sql.Item2);
                StringBuilder wherebuilder = new StringBuilder();
                wherebuilder.AppendFormat("{0} ", sql.Item1);

                if (whereClauses.Count > 1)
                {
                    for (int i = 1; i < whereClauses.Count; i++)
                    {
                        var where = whereClauses[i].BuildSqlString(dialect, i);
                        AddToParameterList(where.Item2);

                        string prep = "AND";
                        if (whereClauses[i].PreOperator != SqlOperator.AND)
                            prep = "OR";

                        wherebuilder.AppendFormat("{0} {1} ", prep, where.Item1);
                    }
                }

                queryBody.WhereExpression = wherebuilder.ToString().Trim();
            }

            if (sortClauses.Count > 0)
            {
                queryBody.SortExpression = string.Join(", ", sortClauses);
            }

            return queryBody;
        }
    }
}

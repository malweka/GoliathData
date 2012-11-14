using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Providers;

namespace Goliath.Data.Sql
{
    class UpdateSqlBuilder : INonQuerySqlBuilder, IBinaryNonQueryOperation
    {
        readonly List<string> columnNames = new List<string>();
        private readonly List<QueryParam> parameters = new List<QueryParam>();
        readonly List<FilterUpdateBuilder> whereClauses = new List<FilterUpdateBuilder>();
        private readonly string tableName;

        private ISession session;
        readonly SqlDialect dialect;

        public UpdateSqlBuilder(string tableName, ISession session, List<string> columnNames, List<QueryParam> parameters)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");

            if (session == null)
                throw new ArgumentNullException("session");

            if ((columnNames == null) || (columnNames.Count == 0))
                throw new ArgumentException("Must provide column to update", "columnNames");

            if ((parameters == null) || (parameters.Count == 0))
                throw new ArgumentException("Must provide column to update", "columnNames");

            this.session = session;
            this.tableName = tableName;
            this.session = session;
            dialect = session.SessionFactory.DataSerializer.SqlDialect;

            this.columnNames = columnNames;
            this.parameters = parameters;
        }

        public string TableName
        {
            get { return tableName; }
        }

        public List<QueryParam> Parameters
        {
            get { return parameters; }
        }

        void AddToParameterList(QueryParam param)
        {
            if (param != null)
            {
                Parameters.Add(param);
            }
        }

        FilterUpdateBuilder BuildWhereClause(SqlOperator preOperator, string columnName)
        {
            var whereBuilder = new FilterUpdateBuilder(this, columnName) { PreOperator = preOperator };
            whereClauses.Add(whereBuilder);
            return whereBuilder;
        }

        public UpdateSqlBody Build()
        {
            var columnFormatter = new SqlSelectColumnFormatter();
            var paramNames = Parameters.Select(queryParam => dialect.CreateParameterName(queryParam.Name)).ToList();

            var sqlBody = new UpdateSqlBody
                              {
                                  Into = TableName,
                                  ColumnEnumeration = columnFormatter.Format(columnNames, null),
                                  ValuesEnumeration = columnFormatter.Format(paramNames, null)
                              };

            if (whereClauses.Count > 0)
            {
                var firstWhere = whereClauses[0];
                var sql = firstWhere.BuildSqlString(dialect, 0);
                AddToParameterList(sql.Item2);
                var wherebuilder = new StringBuilder();
                wherebuilder.AppendFormat("{0} ", sql.Item1);

                if (whereClauses.Count > 1)
                {
                    for (var i = 1; i < whereClauses.Count; i++)
                    {
                        var where = whereClauses[i].BuildSqlString(dialect, i);
                        AddToParameterList(where.Item2);

                        var prep = "AND";
                        if (whereClauses[i].PreOperator != SqlOperator.AND)
                            prep = "OR";

                        wherebuilder.AppendFormat("{0} {1} ", prep, where.Item1);
                    }
                }

                sqlBody.WhereExpression = wherebuilder.ToString().Trim();
            }
            else
            {
                throw new DataAccessException("Update missing where statement. Goliath cannot run an update without filters");
            }

            return sqlBody;
        }

        #region INonQuerySqlBuilder Members

        public IFilterNonQueryClause Where(string propertyName)
        {
            return And(propertyName);
        }

        #endregion

        #region IBinaryNonQueryOperation Members

        public IFilterNonQueryClause And(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            return BuildWhereClause(SqlOperator.AND, propertyName);
        }

        public IFilterNonQueryClause Or(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            return BuildWhereClause(SqlOperator.OR, propertyName);
        }

        #endregion
    }

    public struct UpdateSqlBody
    {
        /// <summary>
        /// Gets or sets the column enumeration.
        /// </summary>
        /// <value>The column enumeration.</value>
        public string ColumnEnumeration { get; set; }

        /// <summary>
        /// Gets or sets the values enumeration.
        /// </summary>
        /// <value>
        /// The values enumeration.
        /// </value>
        public string ValuesEnumeration { get; set; }

        /// <summary>
        /// Gets or sets the into.
        /// </summary>
        /// <value>
        /// The into.
        /// </value>
        public string Into { get; set; }

        /// <summary>
        /// Gets or sets the where expression.
        /// </summary>
        /// <value>The where expression.</value>
        public string WhereExpression { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="dialect">The dialect.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(SqlDialect dialect)
        {
            var sb = new StringBuilder("INSERT INTO ");
            sb.Append(Into);
            sb.AppendFormat("({0}) ", ColumnEnumeration);
            sb.AppendFormat("VALUES ({0}) ", ValuesEnumeration);
            sb.AppendFormat("WHERE {0}", WhereExpression);
            return sb.ToString();
        }
    }
}

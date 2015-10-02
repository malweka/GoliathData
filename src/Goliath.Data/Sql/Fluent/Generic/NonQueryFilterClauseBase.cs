using System;
using System.Text;
using Goliath.Data.Providers;

namespace Goliath.Data.Sql
{
    public abstract class NonQueryFilterClauseBase
    {
        public string LeftColumn { get; protected set; }
        public string RightColumnName { get; protected set; }
        public object RightParamValue { get; protected set; }

        public System.Data.DbType? PropDbType { get; set; }
        public ComparisonOperator BinaryOperation { get; protected set; }
        public SqlOperator PreOperator { get; set; }

        public Tuple<string, QueryParam> BuildSqlString(SqlDialect dialect, int seed = 0)
        {
            QueryParam parameter = null;
            var sql = new StringBuilder();
            sql.AppendFormat("{0} ", dialect.Escape(LeftColumn, EscapeValueType.Column));

            string op = null;
            switch (BinaryOperation)
            {
                case ComparisonOperator.Equal:
                    op = "=";
                    break;
                case ComparisonOperator.GreaterOrEquals:
                    op = ">=";
                    break;
                case ComparisonOperator.GreaterThan:
                    op = ">";
                    break;
                case ComparisonOperator.In:
                    op = "IN";
                    break;
                case ComparisonOperator.IsNotNull:
                    op = "IS NOT NULL";
                    break;
                case ComparisonOperator.IsNull:
                    op = "IS NULL";
                    break;
                case ComparisonOperator.Like:
                    op = "LIKE";
                    break;
                case ComparisonOperator.LowerOrEquals:
                    op = "<=";
                    break;
                case ComparisonOperator.LowerThan:
                    op = "<";
                    break;
                case ComparisonOperator.NotEqual:
                    op = "<>";
                    break;
                case ComparisonOperator.NotLike:
                    op = "NOT LIKE";
                    break;
            }

            sql.AppendFormat("{0} ", op);
            if (!string.IsNullOrWhiteSpace(RightColumnName))
            {
                sql.AppendFormat("{0} ", dialect.Escape(RightColumnName, EscapeValueType.Column));
            }
            else if (RightParamValue != null)
            {
                string paramName = string.Format("qPm{0}", seed);
                parameter = new QueryParam(paramName, PropDbType) { Value = RightParamValue };
                sql.Append(dialect.CreateParameterName(paramName));
            }

            return Tuple.Create(sql.ToString(), parameter);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace Goliath.Data.Sql
{
    using DataAccess;
    using Providers;

    class WhereClauseBuilder : IFilterClause
    {
        QueryBuilder builder;

        public string LeftColumn { get; private set; }

        public object ParamValue { get; set; }

        public string RightColumn { get; set; }

        public System.Data.DbType? PropDbType { get; set; }

        public SqlOperator PreOperator { get; set; }

        public ComparisonOperator BinaryOperation { get; set; }

        bool isRightOperandSet;

        public string TableAlias { get; set; }
        public string RightColumnTableAlias { get; set; }

        public WhereClauseBuilder(QueryBuilder builder, string leftColumn)
        {
            this.builder = builder;
            LeftColumn = leftColumn;

        }

        void BuildBinaryOperation(object value, ComparisonOperator binOp)
        {
            if (!isRightOperandSet)
            {
                ParamValue = value;
                BinaryOperation = binOp;
                isRightOperandSet = true;
            }
        }

        void BuildColumnBinaryOperation(string rightColumn, ComparisonOperator binOp)
        {
            BuildColumnBinaryOperation(string.Empty, rightColumn, binOp);
        }

        void BuildColumnBinaryOperation(string tableAlias, string rightColumn, ComparisonOperator binOp)
        {
            if (!isRightOperandSet)
            {
                RightColumn = rightColumn;
                BinaryOperation = binOp;
                if (!string.IsNullOrWhiteSpace(tableAlias))
                    RightColumnTableAlias = tableAlias;
                isRightOperandSet = true;
            }
        }

        public Tuple<string, QueryParam> BuildSqlString(SqlDialect dialect, int seed)
        {
            QueryParam parameter = null;
            StringBuilder sql = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(TableAlias))
                sql.AppendFormat("{0}.{1} ", TableAlias, LeftColumn);
            else
                sql.AppendFormat("{0} ", LeftColumn);

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
                case ComparisonOperator.LikeNonCaseSensitive:
                    op = dialect.PrintCaseIncensitiveLike();
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
            if (!string.IsNullOrWhiteSpace(RightColumn))
            {
                if (!string.IsNullOrWhiteSpace(RightColumnTableAlias))
                    sql.AppendFormat("{0}.{1} ", RightColumnTableAlias, RightColumn);
                else
                    sql.AppendFormat("{0} ", RightColumn);
            }
            else if (ParamValue != null)
            {
                string paramName = string.Format("qPm{0}", seed);
                parameter = new QueryParam(paramName, PropDbType) { Value = ParamValue };
                sql.Append(dialect.CreateParameterName(paramName));
            }

            return Tuple.Create<string, QueryParam>(sql.ToString(), parameter);
        }

        #region IFilterClause Members

        public IBinaryOperation EqualToValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.Equal);
            return builder;
        }

        public IBinaryOperation EqualTo(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.Equal);
            return builder;
        }

        public IBinaryOperation EqualTo(string tableAlias, string propertyName)
        {
            BuildColumnBinaryOperation(tableAlias, propertyName, ComparisonOperator.Equal);
            return builder;
        }

        public IBinaryOperation GreaterThanValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.GreaterThan);
            return builder;
        }

        public IBinaryOperation GreaterThan(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.GreaterThan);
            return builder;
        }

        public IBinaryOperation GreaterThan(string tableAlias, string propertyName)
        {
            BuildColumnBinaryOperation(tableAlias, propertyName, ComparisonOperator.GreaterThan);
            return builder;
        }


        public IBinaryOperation GreaterOrEqualToValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.GreaterOrEquals);
            return builder;
        }

        public IBinaryOperation GreaterOrEqualTo(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.GreaterOrEquals);
            return builder;
        }

        public IBinaryOperation GreaterOrEqualTo(string tableAlias, string propertyName)
        {
            BuildColumnBinaryOperation(tableAlias, propertyName, ComparisonOperator.GreaterOrEquals);
            return builder;
        }

        public IBinaryOperation LowerOrEqualToValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.LowerOrEquals);
            return builder;
        }

        public IBinaryOperation LowerOrEqualTo(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.LowerOrEquals);
            return builder;
        }

        public IBinaryOperation LowerOrEqualTo(string tableAlias, string propertyName)
        {
            BuildColumnBinaryOperation(tableAlias, propertyName, ComparisonOperator.LowerOrEquals);
            return builder;
        }

        public IBinaryOperation LowerThanValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.LowerThan);
            return builder;
        }

        public IBinaryOperation LowerThan(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.LowerThan);
            return builder;
        }

        public IBinaryOperation LowerThan(string tableAlias, string propertyName)
        {
            BuildColumnBinaryOperation(tableAlias, propertyName, ComparisonOperator.LowerThan);
            return builder;
        }

        public IBinaryOperation LikeValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.Like);
            return builder;
        }

        public IBinaryOperation Like(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.Like);
            return builder;
        }

        public IBinaryOperation Like(string tableAlias, string propertyName)
        {
            BuildColumnBinaryOperation(tableAlias, propertyName, ComparisonOperator.Like);
            return builder;
        }


        public IBinaryOperation ILikeValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.LikeNonCaseSensitive);
            return builder;
        }

        public IBinaryOperation ILike(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.LikeNonCaseSensitive);
            return builder;
        }

        public IBinaryOperation ILike(string tableAlias, string propertyName)
        {
            BuildColumnBinaryOperation(tableAlias, propertyName, ComparisonOperator.LikeNonCaseSensitive);
            return builder;
        }

        #endregion


    }
}

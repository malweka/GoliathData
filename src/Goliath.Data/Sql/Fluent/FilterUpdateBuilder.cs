using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Providers;

namespace Goliath.Data.Sql
{
    class FilterUpdateBuilder : IFilterNonQueryClause
    {
        private readonly IBinaryNonQueryOperation builder;
        public string LeftColumn { get; private set; }

        public object ParamValue { get; set; }

        public string RightColumn { get; set; }

        public SqlOperator PreOperator { get; set; }

        public ComparisonOperator BinaryOperation { get; set; }

        bool isRightOperandSet;

        public FilterUpdateBuilder(IBinaryNonQueryOperation builder, string leftColumn)
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
            if (!isRightOperandSet)
            {
                RightColumn = rightColumn;
                BinaryOperation = binOp;
                isRightOperandSet = true;
            }
        }

        public Tuple<string, QueryParam> BuildSqlString(SqlDialect dialect, int seed)
        {
            QueryParam parameter = null;
            StringBuilder sql = new StringBuilder();
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
            if (!string.IsNullOrWhiteSpace(RightColumn))
            {
                sql.AppendFormat("{0} ", dialect.Escape(RightColumn, EscapeValueType.Column));
            }
            else if (ParamValue != null)
            {
                string paramName = string.Format("qPm{0}", seed);
                parameter = new QueryParam(paramName) { Value = ParamValue };
                sql.Append(dialect.CreateParameterName(paramName));
            }

            return Tuple.Create<string, QueryParam>(sql.ToString(), parameter);
        }

        #region IFilterNonQueryClause Members

        public IBinaryNonQueryOperation EqualToValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.Equal);
            return builder;
        }

        public IBinaryNonQueryOperation EqualTo(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.Equal);
            return builder;
        }

        public IBinaryNonQueryOperation GreaterThanValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.GreaterThan);
            return builder;
        }

        public IBinaryNonQueryOperation GreaterThan(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.GreaterThan);
            return builder;
        }

        public IBinaryNonQueryOperation GreaterOrEqualToValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.GreaterOrEquals);
            return builder;
        }

        public IBinaryNonQueryOperation GreaterOrEqualTo(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.GreaterOrEquals);
            return builder;
        }

        public IBinaryNonQueryOperation LowerOrEqualToValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.LowerOrEquals);
            return builder;
        }

        public IBinaryNonQueryOperation LowerOrEqualTo(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.LowerOrEquals);
            return builder;
        }

        public IBinaryNonQueryOperation LowerThanValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.LowerThan);
            return builder;
        }

        public IBinaryNonQueryOperation LowerThan(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.LowerThan);
            return builder;
        }

        public IBinaryNonQueryOperation LikeValue(object value)
        {
            BuildBinaryOperation(value, ComparisonOperator.Like);
            return builder;
        }

        public IBinaryNonQueryOperation Like(string propertyName)
        {
            BuildColumnBinaryOperation(propertyName, ComparisonOperator.Like);
            return builder;
        }

        #endregion
    }
}

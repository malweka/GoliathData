using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    class WhereClauseBuilder : IFilterClause
    {
        QueryBuilder builder;

        public string LeftColumn { get; private set; }

        public object ParamValue { get; set; }

        public string RightColumn { get; set; }

        public SqlOperator PreOperator { get; set; }

        public ComparisonOperator BinaryOperation { get; set; }

        bool isRightOperandSet;

        public string TableAlias { get; set; }

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
                isRightOperandSet = true;
            }
        }

        #region IFilterClause Members

        public IBinaryOperation EqualTo(object value)
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

        public IBinaryOperation GreaterThan(object value)
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


        public IBinaryOperation GreaterOrEqualTo(object value)
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

        public IBinaryOperation LowerOrEqualTo(object value)
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

        public IBinaryOperation LowerThan(object value)
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

        public IBinaryOperation Like(object value)
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

        #endregion
    }
}

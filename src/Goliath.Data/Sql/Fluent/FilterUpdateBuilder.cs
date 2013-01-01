using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Providers;

namespace Goliath.Data.Sql
{
    class FilterUpdateBuilder : NonQueryFilterClauseBase, IFilterNonQueryClause
    {
        private readonly IBinaryNonQueryOperation builder;

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
                RightParamValue = value;
                BinaryOperation = binOp;
                isRightOperandSet = true;
            }
        }

        void BuildColumnBinaryOperation(string rightColumn, ComparisonOperator binOp)
        {
            if (!isRightOperandSet)
            {
                RightColumnName = rightColumn;
                BinaryOperation = binOp;
                isRightOperandSet = true;
            }
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

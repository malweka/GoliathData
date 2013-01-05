using System;
using System.Linq.Expressions;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    public class NonQueryFilterClause<T> : NonQueryFilterClauseBase, IFilterNonQueryClause<T>
    {
        private readonly IBinaryNonQueryOperation<T> nonQueryBuilder;

        public NonQueryFilterClause(Property property, IBinaryNonQueryOperation<T> nonQueryBuilder)
        {
            LeftColumn = property.ColumnName;
            this.nonQueryBuilder = nonQueryBuilder;
        }

        #region IFilterNonQueryClause<T,TProperty> Members

        public IBinaryNonQueryOperation<T> EqualToValue<TProperty>(TProperty value)
        {
            RightParamValue = value;
            BinaryOperation = ComparisonOperator.Equal;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> EqualTo<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOperation = ComparisonOperator.Equal;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterThanValue<TProperty>(TProperty value)
        {
            RightParamValue = value;
            BinaryOperation = ComparisonOperator.GreaterThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterThan<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOperation = ComparisonOperator.GreaterThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterOrEqualToValue<TProperty>(TProperty value)
        {
            RightParamValue = value;
            BinaryOperation = ComparisonOperator.GreaterOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterOrEqualTo<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOperation = ComparisonOperator.GreaterOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerOrEqualToValue<TProperty>(TProperty value)
        {
            RightParamValue = value;
            BinaryOperation = ComparisonOperator.LowerOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerOrEqualTo<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOperation = ComparisonOperator.LowerOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerThanValue<TProperty>(TProperty value)
        {
            RightParamValue = value;
            BinaryOperation = ComparisonOperator.LowerThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerThan<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOperation = ComparisonOperator.LowerThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LikeValue<TProperty>(TProperty value)
        {
            RightParamValue = value;
            BinaryOperation = ComparisonOperator.Like;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> Like<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOperation = ComparisonOperator.Like;
            return nonQueryBuilder;
        }

        #endregion
    }
}
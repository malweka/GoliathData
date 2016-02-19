using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Goliath.Data.Mapping;

namespace Goliath.Data.Sql
{
    class WhereClauseBuilderWrapper<T, TProperty> : IFilterClause<T, TProperty>
    {

        WhereClauseBuilder whereClause;
        QueryBuilder<T> queryBuilder; 
        public Property LeftColumn { get; private set; }

        public WhereClauseBuilderWrapper(Property property, QueryBuilder<T> queryBuilder, WhereClauseBuilder whereClause)
        {
            LeftColumn = property;
            this.whereClause = whereClause;
            this.queryBuilder = queryBuilder;
            whereClause.PropDbType = property.DbType;
        }

        IBinaryOperation<T> BuildBinaryOperation(ComparisonOperator binaryOp, Expression<Func<T, TProperty>> property)
        {
            var prop = queryBuilder.ExtractProperty(property);
            string columnName = string.Empty;
            string tableAlias = string.Empty;

            if (prop is Relation)
            {
                Relation rel = (Relation)prop;
                var relEntityMap = queryBuilder.Table.Parent.GetEntityMap(rel.ReferenceEntityName);
                tableAlias = relEntityMap.TableAlias;
                columnName = rel.ColumnName;
            }
            else
            {
                tableAlias = queryBuilder.Table.TableAlias;
                columnName = prop.ColumnName;
            }


            switch (binaryOp)
            {
                case ComparisonOperator.Equal:
                    whereClause.EqualTo(tableAlias, columnName);
                    break;
                case ComparisonOperator.GreaterOrEquals:
                    whereClause.GreaterOrEqualTo(tableAlias, columnName);
                    break;
                case ComparisonOperator.GreaterThan:
                    whereClause.GreaterThan(tableAlias, columnName);
                    break;
                case ComparisonOperator.In:
                    throw new NotSupportedException("IN is not supported");
                //break;
                case ComparisonOperator.IsNotNull:
                    throw new NotSupportedException("Is Not Null is not supported");
                //break;
                case ComparisonOperator.IsNull:
                    throw new NotSupportedException("Is Null is not supported");
                //break;
                case ComparisonOperator.Like:
                    whereClause.Like(tableAlias, columnName);
                    break;
                case ComparisonOperator.LikeNonCaseSensitive:
                    whereClause.ILike(tableAlias, columnName);
                    break;
                case ComparisonOperator.LowerOrEquals:
                    whereClause.LowerOrEqualTo(tableAlias, columnName);
                    break;
                case ComparisonOperator.LowerThan:
                    whereClause.LowerThan(tableAlias, columnName);
                    break;
                case ComparisonOperator.NotEqual:
                    throw new NotSupportedException("Not Equalis not supported");
                //break;
                case ComparisonOperator.NotLike:
                    throw new NotSupportedException("Not Like is not supported");
                //break;
            }

            return queryBuilder;
        }

        IBinaryOperation<T> BuildBinaryOperation(ComparisonOperator binaryOp, TProperty value)
        {
            switch (binaryOp)
            {
                case ComparisonOperator.Equal:
                    whereClause.EqualToValue(value);
                    break;
                case ComparisonOperator.GreaterOrEquals:
                    whereClause.GreaterOrEqualToValue(value);
                    break;
                case ComparisonOperator.GreaterThan:
                    whereClause.GreaterThanValue(value);
                    break;
                case ComparisonOperator.In:
                    throw new NotSupportedException("IN is not supported");
                //break;
                case ComparisonOperator.IsNotNull:
                    whereClause.IsNotNull();
                    break;
                //break;
                case ComparisonOperator.IsNull:
                    whereClause.IsNull();
                    break;
                //break;
                case ComparisonOperator.Like:
                    whereClause.LikeValue(value);
                    break;
                case ComparisonOperator.LikeNonCaseSensitive:
                    whereClause.ILikeValue(value);
                    break;
                case ComparisonOperator.LowerOrEquals:
                    whereClause.LowerOrEqualToValue(value);
                    break;
                case ComparisonOperator.LowerThan:
                    whereClause.LowerThanValue(value);
                    break;
                case ComparisonOperator.NotEqual:
                    throw new NotSupportedException("Not Equalis not supported");
                //break;
                case ComparisonOperator.NotLike:
                    throw new NotSupportedException("Not Like is not supported");
                //break;
            }

            return queryBuilder;
        }

        #region IFilterClause<T,TProperty> Members

        public IBinaryOperation<T> EqualToValue(TProperty value)
        {
            return BuildBinaryOperation(ComparisonOperator.Equal, value);
        }

        public IBinaryOperation<T> EqualTo(System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            return BuildBinaryOperation(ComparisonOperator.Equal, property);
        }

        public IBinaryOperation<T> GreaterThanValue(TProperty value)
        {
            return BuildBinaryOperation(ComparisonOperator.GreaterThan, value);
        }

        public IBinaryOperation<T> GreaterThan(System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            return BuildBinaryOperation(ComparisonOperator.GreaterThan, property);
        }

        public IBinaryOperation<T> GreaterOrEqualToValue(TProperty value)
        {
            return BuildBinaryOperation(ComparisonOperator.GreaterOrEquals, value);
        }

        public IBinaryOperation<T> GreaterOrEqualTo(System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            return BuildBinaryOperation(ComparisonOperator.GreaterOrEquals, property);
        }

        public IBinaryOperation<T> LowerOrEqualToValue(TProperty value)
        {
            return BuildBinaryOperation(ComparisonOperator.LowerOrEquals, value);
        }

        public IBinaryOperation<T> LowerOrEqualTo(System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            return BuildBinaryOperation(ComparisonOperator.LowerOrEquals, property);
        }

        public IBinaryOperation<T> LowerThanValue(TProperty value)
        {
            return BuildBinaryOperation(ComparisonOperator.LowerThan, value);
        }

        public IBinaryOperation<T> LowerThan(Expression<Func<T, TProperty>> property)
        {
            return BuildBinaryOperation(ComparisonOperator.LowerThan, property);
        }

        public IBinaryOperation<T> LikeValue(TProperty value)
        {
            return BuildBinaryOperation(ComparisonOperator.Like, value);
        }

        public IBinaryOperation<T> Like(Expression<Func<T, TProperty>> property)
        {
            return BuildBinaryOperation(ComparisonOperator.Like, property);
        }

        public IBinaryOperation<T> ILikeValue(TProperty value)
        {
            return BuildBinaryOperation(ComparisonOperator.LikeNonCaseSensitive, value);
        }

        public IBinaryOperation<T> ILike(Expression<Func<T, TProperty>> property)
        {
            return BuildBinaryOperation(ComparisonOperator.LikeNonCaseSensitive, property);
        }

        public IBinaryOperation<T> IsNotNull()
        {
            return BuildBinaryOperation(ComparisonOperator.IsNotNull, null);
        }

        public IBinaryOperation<T> IsNull()
        {
            return BuildBinaryOperation(ComparisonOperator.IsNull, null);
        }

        #endregion
    }
}

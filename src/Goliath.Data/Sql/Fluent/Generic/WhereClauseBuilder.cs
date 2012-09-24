using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Goliath.Data.Sql
{
    using Mapping;
    using Utils;

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
        }

        IBinaryOperation<T> BuildBinaryOperation(ComparisonOperator binaryOp, System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            switch (binaryOp)
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
        }

        #region IFilterClause<T,TProperty> Members

        public IBinaryOperation<T> EqualToValue(TProperty value)
        {
            whereClause.EqualToValue(value);
            return queryBuilder;
        }

        public IBinaryOperation<T> EqualTo(System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            var prop = queryBuilder.ExtractProperty(property);
            if (prop is Relation)
            {
                Relation rel = (Relation)prop;
                var relEntityMap = queryBuilder.Table.Parent.GetEntityMap(rel.ReferenceEntityName);
                whereClause.EqualTo(relEntityMap.TableAlias, rel.ColumnName);
            }
            else
                whereClause.EqualTo(queryBuilder.Table.TableAlias, prop.ColumnName);
            return queryBuilder;
        }

        public IBinaryOperation<T> GreaterThanValue(TProperty value)
        {
            whereClause.GreaterThanValue(value);
            return queryBuilder;
        }

        public IBinaryOperation<T> GreaterThan(System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            var prop = queryBuilder.ExtractProperty(property);
            if (prop is Relation)
            {
                Relation rel = (Relation)prop;
                var relEntityMap = queryBuilder.Table.Parent.GetEntityMap(rel.ReferenceEntityName);
                whereClause.GreaterThan(relEntityMap.TableAlias, rel.ColumnName);
            }
            else
                whereClause.GreaterThan(queryBuilder.Table.TableAlias, prop.ColumnName);
            return queryBuilder;
        }

        public IBinaryOperation<T> GreaterOrEqualToValue(TProperty value)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> GreaterOrEqualTo(System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> LowerOrEqualToValue(TProperty obj)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> LowerOrEqualTo(System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> LowerThanValue(TProperty obj)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> LowerThan(System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> LikeValue(TProperty param)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> Like(System.Linq.Expressions.Expression<Func<T, TProperty>> propertye)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

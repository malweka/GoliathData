using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Goliath.Data.Sql
{
    partial class QueryBuilder<T>
    {
        WhereClauseBuilderWrapper<T, TProperty> BuildWhereClause<TProperty>(SqlOperator preOperator, Expression<Func<T, TProperty>> property)
        {
            var prop = ExtractProperty(property);
            WhereClauseBuilder wcBuilder;
            if (preOperator == SqlOperator.AND)
                wcBuilder = innerBuilder.And(Table.TableAlias, prop.ColumnName) as WhereClauseBuilder;
            else
                wcBuilder = innerBuilder.Or(Table.TableAlias, prop.ColumnName) as WhereClauseBuilder;

            WhereClauseBuilderWrapper<T, TProperty> whereBuilder = new WhereClauseBuilderWrapper<T, TProperty>(prop, this, wcBuilder);

            return whereBuilder;

        }

        #region IQueryBuilder<T> Members

        public IFilterClause<T, TProperty> Where<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return BuildWhereClause<TProperty>(SqlOperator.AND, property);
        }

        public IFilterClause<T, TProperty> And<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return BuildWhereClause<TProperty>(SqlOperator.AND, property);
        }

        public IFilterClause<T, TProperty> Or<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return BuildWhereClause<TProperty>(SqlOperator.OR, property);
        }

        #endregion

        public IFilterClause Where(string propertyName)
        {
            return innerBuilder.Where(propertyName);
        }
    }
}

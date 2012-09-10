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

        //Property ExtractProperty(Expression<Func<T, TProperty>> prototype)
        //{
        //    var propertName = prototype.GetMemberName();
        //    var prop = queryBuilder.Table[propertName];
        //    if (prop == null)
        //        throw new GoliathDataException(string.Format("Could not find property {0}. {0} was not mapped properly.", propertName));

        //    return prop;
        //}

        #region IFilterClause<T,TProperty> Members

        public IBinaryOperation<T> EqualToValue(TProperty value)
        {
            whereClause.EqualToValue(value);
            return queryBuilder;
        }

        public IBinaryOperation<T> EqualTo(System.Linq.Expressions.Expression<Func<T, TProperty>> prototype)
        {
            var prop = queryBuilder.ExtractProperty(prototype);
            whereClause.EqualTo(queryBuilder.Table.TableAlias, prop.ColumnName);
            return queryBuilder;
        }

        public IBinaryOperation<T> GreaterThanValue(TProperty value)
        {
            //this doesn't build by design so i know where i stopped.
            whereClause.EqualToValue
                //handle Relation
        }

        public IBinaryOperation<T> GreaterThan(System.Linq.Expressions.Expression<Func<T, TProperty>> prototype)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> GreaterOrEqualToValue(TProperty value)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> GreaterOrEqualTo(System.Linq.Expressions.Expression<Func<T, TProperty>> prototype)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> LowerOrEqualToValue(TProperty obj)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> LowerOrEqualTo(System.Linq.Expressions.Expression<Func<T, TProperty>> prototype)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> LowerThanValue(TProperty obj)
        {
            throw new NotImplementedException();
        }

        public IBinaryOperation<T> LowerThan(System.Linq.Expressions.Expression<Func<T, TProperty>> prototype)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;


namespace Goliath.Data.Sql
{
    using Mapping;
    using Utils;

    class JoinBuilder<T, TRelation> : IJoinable<T, TRelation>, IJoinOperation<T>
    {
        QueryBuilder<T> queryBuilder;
        JoinBuilder joinBuilder;
        EntityMap joinMap;

        public JoinBuilder(QueryBuilder<T> queryBuilder, JoinBuilder joinBuilder, EntityMap joinMap)
        {
            this.queryBuilder = queryBuilder;
            this.joinBuilder = joinBuilder;
            this.joinMap = joinMap;
        }


        #region IJoinable<T,TRelation> Members

        public IJoinOperation<T> On<TProperty>(System.Linq.Expressions.Expression<Func<TRelation, TProperty>> property)
        {
            var propName = property.GetMemberName();            

            var prop = joinMap[propName];
            
            if(prop == null)
                throw new GoliathDataException(string.Format("Could not find property {0}. {0} was not mapped properly.", propName));

            joinBuilder.On(prop.ColumnName);
            return this;

        }

        public IQueryBuilder<T> EqualTo<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> property)
        {
            var propName = property.GetMemberName();
            var prop = queryBuilder.Table[propName];

            if (prop == null)
                throw new GoliathDataException(string.Format("Could not find property {0}. {0} was not mapped properly.", propName));

            joinBuilder.EqualTo(prop.ColumnName);
            return queryBuilder;
        }

        #endregion
    }
}

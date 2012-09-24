using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Goliath.Data.Sql
{
    using Mapping;
    using Utils;

    class QueryBuilder<T> : IQueryBuilder<T>, IBinaryOperation<T>
    {
        ISession session;
        QueryBuilder innerBuilder;

        public QueryBuilder(ISession session)
            : this(session, new List<string>() { })
        {

        }

        public QueryBuilder(ISession session, List<string> propertyNames)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            this.session = session;
            Load(propertyNames);
        }

        public EntityMap Table { get; private set; }
        public EntityMap Extends { get; private set; }

        internal Property ExtractProperty<TProperty>(Expression<Func<T, TProperty>> prototype)
        {
            var propertName = prototype.GetMemberName();

            var prop = Table[propertName];
            if (prop == null)
                throw new GoliathDataException(string.Format("Could not find property {0}. {0} was not mapped properly.", propertName));

            if (prop is Relation)
            {
                Relation rel = (Relation)prop;
                var relEntity = session.SessionFactory.DbSettings.Map.GetEntityMap(rel.ReferenceEntityName);
                if (!innerBuilder.Joins.ContainsKey(relEntity.TableAlias))
                    innerBuilder.LeftJoin(relEntity.TableName, relEntity.TableAlias).On(prop.ColumnName).EqualTo(rel.ColumnName);
            }

            return prop;
        }

        void Load(List<string> propertyNames)
        {
            string typeFullName = typeof(T).FullName;
            Table = session.SessionFactory.DbSettings.Map.GetEntityMap(typeFullName);

            innerBuilder = new QueryBuilder(session, propertyNames);
            innerBuilder.From(Table.TableName, Table.TableAlias);

            if (Table.IsSubClass)
            {
                if (Table.PrimaryKey == null)
                    throw new GoliathDataException(string.Format("Cannot resolve innheritance between {0} and {1}. Not primary key is defined.", typeFullName, Table.Extends));

                //let's get parent and add joins
                Extends = session.SessionFactory.DbSettings.Map.GetEntityMap(Table.Extends);

                foreach (var pk in Extends.PrimaryKey.Keys)
                {
                    var k = Table.PrimaryKey.Keys[pk.Key.Name];
                    innerBuilder.InnerJoin(Extends.TableName, Extends.TableAlias).On(k.Key.ColumnName).EqualTo(pk.Key.ColumnName);
                }

            }


        }

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

        #region IQueryFetchable<T> Members

        public IFetchable<T> Limit(int i)
        {
            throw new NotImplementedException();
        }

        public IFetchable<T> Offset(int i)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IFetchable<T> Members

        public ICollection<T> FetchAll()
        {
            throw new NotImplementedException();
        }

        public T FetchOne()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBinaryOperation<T> Members


        public ISorterClause<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> property)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

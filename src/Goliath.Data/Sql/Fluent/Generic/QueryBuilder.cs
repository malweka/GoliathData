using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Goliath.Data.Sql
{
    using Mapping;
    using Utils;

    partial class QueryBuilder<T> : IQueryBuilder<T>, IBinaryOperation<T>
    {
        ISession session;
        QueryBuilder innerBuilder;
        public EntityMap Table { get; private set; }
        public EntityMap Extends { get; private set; }
        public QueryBuilder InnerBuilder { get { return innerBuilder; } }

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

         MapConfig GetMapConfig()
        {
            return session.SessionFactory.DbSettings.Map;
        }
        
        internal Property ExtractProperty<TProperty>(Expression<Func<T, TProperty>> property)
        {
            var propertName = property.GetMemberName();

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

        JoinBuilder<T, TRelation> BuildJoinBuilder<TRelation>(JoinType joinType)
        {
            JoinBuilder jbuilder = new JoinBuilder(innerBuilder, Table.TableName, null, null);
            var map = GetMapConfig();
            EntityMap joinMap = map.GetEntityMap(typeof(TRelation).FullName);
            switch(joinType)
            {
                case JoinType.Inner:
                    innerBuilder.InnerJoin(joinMap.TableName, joinMap.TableAlias);
                    break;
                case JoinType.Full:
                    break;
                case JoinType.Left:
                    innerBuilder.LeftJoin(joinMap.TableName, joinMap.TableAlias);
                    break;
                case JoinType.Right:
                    innerBuilder.RightJoin(joinMap.TableName, joinMap.TableAlias);
                    break;
            }
            return new JoinBuilder<T, TRelation>(this, jbuilder, joinMap);
        }

        public IJoinable<T, TRelation> InnerJoin<TRelation>()
        {
            return BuildJoinBuilder<TRelation>(JoinType.Inner);
        }

        public IJoinable<T, TRelation> LeftJoin<TRelation>()
        {
            return BuildJoinBuilder<TRelation>(JoinType.Left);
        }

        public IJoinable<T, TRelation> RightJoin<TRelation>()
        {
            return BuildJoinBuilder<TRelation>(JoinType.Right);
        }


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

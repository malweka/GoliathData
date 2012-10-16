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
                if (!rel.LazyLoad)
                {
                    var relEntity = session.SessionFactory.DbSettings.Map.GetEntityMap(rel.ReferenceEntityName);
                    if (!innerBuilder.Joins.ContainsKey(relEntity.TableAlias))
                        innerBuilder.LeftJoin(relEntity.TableName, relEntity.TableAlias).On(Table.TableAlias, prop.ColumnName).EqualTo(rel.ReferenceColumn);
                }
            }

            return prop;
        }

        public string BuildColumnSelectString(string columnName, string tableAbbreviation)
        {
            return string.Format("{2}.{0} AS {1}", columnName, ParameterNameBuilderHelper.ColumnQueryName(columnName, tableAbbreviation), tableAbbreviation);
        }

        void LoadColumns(EntityMap entityMap, List<string> propertyNames)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();

            foreach (var prop in entityMap)
            {
                if (prop is Relation)
                {
                    Relation rel = (Relation)prop;
                    if (rel.RelationType == RelationshipType.ManyToOne)
                    {
                        if (!cols.ContainsKey(rel.ColumnName))
                            cols.Add(rel.ColumnName, BuildColumnSelectString(rel.ColumnName, entityMap.TableAlias));

                        if (!rel.LazyLoad)
                        {
                            var relEnt = session.SessionFactory.DbSettings.Map.GetEntityMap(rel.ReferenceEntityName);
                            if (!innerBuilder.Joins.ContainsKey(relEnt.TableAlias))
                            {
                                innerBuilder.LeftJoin(relEnt.TableName, relEnt.TableAlias).On(entityMap.TableAlias, rel.ReferenceColumn).EqualTo(prop.ColumnName);
                            }
                            LoadColumns(relEnt, propertyNames);
                            
                        }
                    }
                }
                else
                    cols.Add(prop.ColumnName, BuildColumnSelectString(prop.ColumnName, entityMap.TableAlias));
            }

            propertyNames.AddRange(cols.Values);
        }

        void Load(List<string> propertyNames)
        {
            string typeFullName = typeof(T).FullName;
            Table = session.SessionFactory.DbSettings.Map.GetEntityMap(typeFullName);
            innerBuilder = new QueryBuilder(session, propertyNames);
            innerBuilder.From(Table.TableName, Table.TableAlias);

            if (propertyNames == null)
                propertyNames = new List<string>();

            if (propertyNames.Count == 0)
            {
                LoadColumns(Table, propertyNames);
            }            

            if (Table.IsSubClass)
            {
                if (Table.PrimaryKey == null)
                    throw new GoliathDataException(string.Format("Cannot resolve innheritance between {0} and {1}. Not primary key is defined.", typeFullName, Table.Extends));

                //let's get parent and add joins
                Extends = session.SessionFactory.DbSettings.Map.GetEntityMap(Table.Extends);

                foreach (var pk in Extends.PrimaryKey.Keys)
                {
                    var k = Table.PrimaryKey.Keys[pk.Key.Name];
                    innerBuilder.InnerJoin(Extends.TableName, Extends.TableAlias).On(Table.TableAlias, k.Key.ColumnName).EqualTo(pk.Key.ColumnName);
                }

            }


        }

        JoinBuilder<T, TRelation> BuildJoinBuilder<TRelation>(JoinType joinType)
        {
            JoinBuilder jbuilder = new JoinBuilder(innerBuilder, Table.TableName, null, null);
            var map = GetMapConfig();
            EntityMap joinMap = map.GetEntityMap(typeof(TRelation).FullName);
            switch (joinType)
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

        public IQueryFetchable<T> Limit(int i)
        {
            innerBuilder.Limit(1);
            return this;
        }

        public IQueryFetchable<T> Offset(int i)
        {
            innerBuilder.Offset(i);
            return this;
        }

        #endregion

        #region IFetchable<T> Members

        public ICollection<T> FetchAll()
        {
            return innerBuilder.FetchAll<T>();
        }

        public T FetchOne()
        {
            return innerBuilder.FetchOne<T>();
        }

        #endregion

        #region IBinaryOperation<T> Members


        public ISorterClause<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> property)
        {
            var prop = ExtractProperty(property);

            var sortBuilder = (SortBuilder)innerBuilder.OrderBy(prop.ColumnName);
            return new SortBuilder<T>(this, sortBuilder);
        }

        #endregion
    }
}

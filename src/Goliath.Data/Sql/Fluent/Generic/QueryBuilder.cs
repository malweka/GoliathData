using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Goliath.Data.Sql
{
    using Mapping;
    using Utils;

    partial class QueryBuilder<T> : IQueryBuilder<T>, IBinaryOperation<T>, IFetchableWithOutput<T>
    {
        ISession session;
        QueryBuilder innerBuilder;
        private TableQueryMap queryMap;

        public EntityMap Table { get; private set; }
        //public EntityMap Extends { get; private set; }

        public QueryBuilder InnerBuilder { get { return innerBuilder; } }

        public TableQueryMap QueryMap
        {
            get { return queryMap; }
        }

        //public QueryBuilder(ISession session)
        //    : this(session, new List<string>() { })
        //{

        //}

        public QueryBuilder(ISession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            this.session = session;

            Load();
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

            //if (prop is Relation)
            //{
            //    Relation rel = (Relation)prop;
            //    if (!rel.LazyLoad)
            //    {
            //        var relEntity = session.SessionFactory.DbSettings.Map.GetEntityMap(rel.ReferenceEntityName);

            //        JoinBuilder jn;
            //        if (!innerBuilder.Joins.TryGetValue(relEntity.TableAlias, out jn))
            //        {
            //            innerBuilder.LeftJoin(relEntity.TableName, relEntity.TableAlias + rel.InternalIndex).On(Table.TableAlias, prop.ColumnName).EqualTo(rel.ReferenceColumn);
            //        }
            //        //else
            //        //{
            //        //    if(!prop.ColumnName.Equals(jn.JoinLeftColumn))
            //        //    {

            //        //    }

            //        //}
            //    }
            //}

            return prop;
        }

        public string BuildColumnSelectString(string columnName, string tableAbbreviation)
        {
            return string.Format("{1}.{0} AS {1}.{0}", columnName, tableAbbreviation);
        }



        void Load()
        {
            string typeFullName = typeof(T).FullName;

            Table = session.SessionFactory.DbSettings.Map.GetEntityMap(typeFullName);
            int iteration = 0;
            int recursion = 0;
            queryMap = new TableQueryMap(Table.FullName, ref recursion,ref iteration);

            var columnList = new List<string>();

            //queryMap.LoadColumns(Table, session, columnList);

            innerBuilder = new QueryBuilder(session, columnList);
            innerBuilder.From(Table.TableName, queryMap.Prefix);
            queryMap.LoadColumns(Table, session, innerBuilder, columnList);

            innerBuilder.QueryMap = QueryMap;
        }

        JoinBuilder<T, TRelation> BuildJoinBuilder<TRelation>(JoinType joinType)
        {
            JoinBuilder jbuilder = new JoinBuilder(innerBuilder, Table.TableName, null, null);
            var map = GetMapConfig();

            EntityMap joinMap = map.GetEntityMap(typeof(TRelation).FullName);

            var count = Table.Relations.Count;
            if (Table.IsSubClass)
                count = count + 1;

            var prefix = TableQueryMap.CreatePrefix(count + 1, 2);

            switch (joinType)
            {
                case JoinType.Inner:
                    innerBuilder.InnerJoin(joinMap.TableName, prefix);
                    break;
                case JoinType.Full:
                    break;
                case JoinType.Left:
                    innerBuilder.LeftJoin(joinMap.TableName, prefix);
                    break;
                case JoinType.Right:
                    innerBuilder.RightJoin(joinMap.TableName, prefix);
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

        public IFetchableWithOutput<T> Take(int limit, int offset)
        {
            innerBuilder.Take(limit, offset);
            return this;
        }

        #endregion

        #region IFetchable<T> Members

        public ICollection<T> FetchAll(out long total)
        {
            return innerBuilder.FetchAll<T>(out total);
        }

        public ICollection<T> FetchAll()
        {
            return innerBuilder.FetchAll<T>();
        }

        public int Count()
        {
            return innerBuilder.Count();
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

        public ISorterClause<T> OrderBy(string columnName)
        {

            var sortBuilder = (SortBuilder)innerBuilder.OrderBy(columnName);
            return new SortBuilder<T>(this, sortBuilder);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    public abstract class NonQuerySqlBuilderBase<T> : INonQuerySqlBuilder<T>, IBinaryNonQueryOperation<T>
    {
        protected ISession session;
        protected T entity;
        protected Type entityType;
        protected EntityMap Table { get; set; }
        public List<NonQueryFilterClause<T>> Filters { get; private set; }
        protected readonly List<QueryParam> whereParameters = new List<QueryParam>();

        protected NonQuerySqlBuilderBase(ISession session, T entity) : this(session, session.SessionFactory.DbSettings.Map.GetEntityMap(entity.GetType().FullName), entity)
        {
        }

        protected string BuildWhereExpression(SqlDialect dialect)
        {
            if (Filters.Count > 0)
            {
                var firstWhere = Filters[0];
                var sql = firstWhere.BuildSqlString(dialect);


                var wherebuilder = new StringBuilder(sql.Item1 + " ");
                if (sql.Item2 != null)
                {
                    whereParameters.Add(sql.Item2);
                }

                if (Filters.Count > 1)
                {
                    for (var i = 1; i < Filters.Count; i++)
                    {
                        var where = Filters[i].BuildSqlString(dialect, i);
                        if (where.Item2 != null)
                            whereParameters.Add(where.Item2);

                        var prep = "AND";
                        if (Filters[i].PreOperator != SqlOperator.AND)
                            prep = "OR";

                        wherebuilder.AppendFormat("{0} {1} ", prep, where.Item1);
                    }
                }

                return wherebuilder.ToString();
            }

            throw new DataAccessException("Update missing where statement. Goliath cannot run an update without filters");
        }

        protected NonQuerySqlBuilderBase(ISession session, EntityMap entityMap, T entity)
        {
            this.session = session;
            this.entity = entity;
            entityType = typeof(T);
            Filters = new List<NonQueryFilterClause<T>>();
            Table = entityMap;
        }

        protected NonQueryFilterClause<T> CreateFilter(EntityMap map, SqlOperator preOperator, string propertyName)
        {
            var prop = map.GetProperty(propertyName);

            //NOTE: we're not supporting this yet. so we shouldn't really be going down to the parent class yet.
            if (prop == null)
            {
                //if (map.IsSubClass)
                //{
                //    var parent = session.SessionFactory.DbSettings.Map.GetEntityMap(map.Extends);
                //    return CreateFilter(parent, preOperator, propertyName);
                //}
                //else
                //{
                throw new MappingException(string.Format("Could not find property {0} on mapped entity {1}", propertyName, map.FullName));
                //}

            }

            var filter = new NonQueryFilterClause<T>(prop, this) { PreOperator = preOperator };
            Filters.Add(filter);
            return filter;
        }

        #region INonQuerySqlBuilder<T> Members

        public virtual IFilterNonQueryClause<T> Where(string propertyName)
        {
            return CreateFilter(Table, SqlOperator.AND, propertyName);
        }

        public virtual IFilterNonQueryClause<T> Where<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return CreateFilter(Table, SqlOperator.AND, property.GetMemberName());
        }

        #endregion

        #region IBinaryNonQueryOperation<T> Members

        public virtual IFilterNonQueryClause<T> And<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return CreateFilter(Table, SqlOperator.AND, property.GetMemberName());
        }

        public virtual IFilterNonQueryClause<T> And(string propertyName)
        {
            return CreateFilter(Table, SqlOperator.AND, propertyName);
        }

        public virtual IFilterNonQueryClause<T> Or<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return CreateFilter(Table, SqlOperator.OR, property.GetMemberName());
        }

        public virtual IFilterNonQueryClause<T> Or(string propertyName)
        {
            return CreateFilter(Table, SqlOperator.OR, propertyName);
        }

        public abstract int Execute();

        #endregion
    }
}
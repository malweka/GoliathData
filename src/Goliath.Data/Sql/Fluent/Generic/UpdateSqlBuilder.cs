using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Goliath.Data.Collections;
using Goliath.Data.DataAccess;
using Goliath.Data.Entity;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    class UpdateSqlBuilder<T> : INonQuerySqlBuilder<T>, IBinaryNonQueryOperation<T>
    {
        private ISession session;
        private T entity;

        private EntityMap Table { get; set; }
        public List<NonQueryFilterClause<T>> Filters { get; private set; }
        readonly List<QueryParam> whereParameters = new List<QueryParam>();

        public UpdateSqlBuilder(ISession session, T entity)
        {
            this.session = session;
            this.entity = entity;
            var type = typeof(T);
            Filters = new List<NonQueryFilterClause<T>>();
            Table = session.SessionFactory.DbSettings.Map.GetEntityMap(type.FullName);

        }

        void LoadColumns(UpdateSqlExecutionList execList, EntityMap entityMap, EntityAccessor accessor)
        {
            var updateBodyInfo = new UpdateSqlBodyInfo();

            var trackable = entity as ITrackable;
            if (trackable != null)
            {
                var changes = trackable.ChangeTracker.GetChangedItems();
                foreach (var item in changes)
                {
                    var prop = entityMap.GetProperty(item.ItemName);
                    if (prop == null)
                    {
                        if (entityMap.IsSubClass && !execList.Statements.ContainsKey(entityMap.Extends))
                        {
                            var parentMap = session.SessionFactory.DbSettings.Map.GetEntityMap(entityMap.Extends);
                            LoadColumns(execList, parentMap, accessor);
                        }
                        else
                            continue;
                    }

                    if (prop == null || prop.IgnoreOnUpdate || prop.IsPrimaryKey)
                        continue;

                    var propInfo = accessor.Properties[prop.Name];
                    if (propInfo == null)
                        throw new MappingException("Could not find mapped property " + prop.Name + " inside " + entityMap.FullName);

                    AddColumnAndParameterToUpdateInfo(execList, updateBodyInfo, entityMap, prop, propInfo, accessor);
                }
            }
            else
            {
                foreach (var pInfo in accessor.Properties)
                {
                    var prop = entityMap.GetProperty(pInfo.Value.PropertyName);
                    if (prop == null)
                    {
                        if (entityMap.IsSubClass && !execList.Statements.ContainsKey(entityMap.Extends))
                        {
                            var parentMap = session.SessionFactory.DbSettings.Map.GetEntityMap(entityMap.Extends);
                            LoadColumns(execList, parentMap, accessor);
                        }
                        else
                            continue;
                    }

                    if (prop == null || prop.IgnoreOnUpdate || prop.IsPrimaryKey)
                        continue;

                    AddColumnAndParameterToUpdateInfo(execList, updateBodyInfo, entityMap, prop, pInfo.Value, accessor);
                }
            }

            execList.Statements.Add(entityMap.FullName, updateBodyInfo);

        }

        void AddColumnAndParameterToUpdateInfo(UpdateSqlExecutionList execList, UpdateSqlBodyInfo updateBodyInfo, EntityMap entityMap, Property prop, PropertyAccessor propInfo, EntityAccessor accessor)
        {
            object val = propInfo.GetMethod(entity);
            bool isRel = false;
            if (prop is Relation)
            {
                var rel = (Relation)prop;
                if (updateBodyInfo.Columns.ContainsKey(prop.ColumnName))
                    return;

                isRel = true;
                if (val != null)
                {
                    if (rel.RelationType == RelationshipType.ManyToOne)
                    {
                        var store = new EntityAccessorStore();
                        var relMap = session.SessionFactory.DbSettings.Map.GetEntityMap(rel.ReferenceEntityName);
                        var relAccessor = store.GetEntityAccessor(val.GetType(), relMap);

                        var relPinfo = relAccessor.Properties[rel.ReferenceProperty];

                        if (relPinfo == null)
                            throw new MappingException(string.Format("could not find property {0} in mapped entity {1}", rel.ReferenceProperty, relMap.FullName));

                        val = relPinfo.GetMethod(val);
                    }
                    else if (rel.RelationType == RelationshipType.ManyToMany)
                    {
                        var trackableCollection = val as ITrackableCollection;
                        if (trackableCollection != null)
                        {
                            AddInsertManyToManyOperation(execList, trackableCollection.InsertedItems, rel, accessor, true);
                            AddInsertManyToManyOperation(execList, trackableCollection.DeletedItems, rel, accessor, false);
                        }

                        //NOTE: if not trackable collection used mapped statement to add or remove many to many associations
                    }
                    else return;
                }
            }
            else
            {
                Tuple<QueryParam, bool> etuple;
                if (updateBodyInfo.Columns.TryGetValue(prop.ColumnName, out etuple))
                {
                    if (etuple.Item2)
                    {
                        updateBodyInfo.Columns.Remove(prop.ColumnName);
                    }
                    else return;
                }
            }

            Tuple<QueryParam, bool> tuple = Tuple.Create(new QueryParam(ParameterNameBuilderHelper.ColumnWithTableAlias(entityMap.TableAlias, prop.ColumnName)) { Value = val }, isRel);
            updateBodyInfo.Columns.Add(prop.ColumnName, tuple);
        }

        void AddInsertManyToManyOperation(UpdateSqlExecutionList execList, IEnumerable insertedItems, Relation rel, EntityAccessor accessor, bool isInsertStatement)
        {
            if (insertedItems != null)
            {
                var relMap = session.SessionFactory.DbSettings.Map.GetEntityMap(rel.ReferenceEntityName);
                var store = new EntityAccessorStore();

                var propInfo = accessor.Properties[rel.MapPropertyName];
                if (propInfo == null)
                    throw new GoliathDataException(string.Format("Could not retrieve value of property {0} in mapped entity {1} with clr type {2}.",
                                rel.MapPropertyName, Table.FullName, typeof(T).FullName));

                var propValue = propInfo.GetMethod(entity);

                PropertyAccessor relPropInfo = null;
                foreach (var item in insertedItems)
                {
                    if (relPropInfo == null)
                    {
                        var relAccessor = store.GetEntityAccessor(item.GetType(), relMap);
                        if (!relAccessor.Properties.TryGetValue(rel.ReferenceProperty, out relPropInfo))
                            throw new GoliathDataException(string.Format("Could not retrieve value of property {0} in mapped entity {1} with clr type {2}.",
                                rel.ReferenceProperty, relMap.FullName, item.GetType().FullName));
                    }

                    var relParam = new QueryParam(rel.MapReferenceColumn) { Value = relPropInfo.GetMethod(item) };
                    var propParm = new QueryParam(rel.MapColumn) { Value = propValue };
                    var dialect = session.SessionFactory.DbSettings.SqlDialect;
                    if (isInsertStatement)
                    {
                        var sql = string.Format("INSERT INTO {0} ({1}, {2}) VALUES({3},{4})",
                                                rel.MapTableName, rel.MapColumn, rel.MapReferenceColumn, dialect.CreateParameterName(propParm.Name), dialect.CreateParameterName(relParam.Name));

                        execList.ManyToManyStatements.Add(Tuple.Create(sql, new List<QueryParam>{ propParm, relParam }));
                    }
                    else
                    {
                        var sql = string.Format("DELETE FROM {0} WHERE {1} = {3} AND {2} = {4}",
                                                rel.MapTableName, rel.MapColumn, rel.MapReferenceColumn, dialect.CreateParameterName(propParm.Name), dialect.CreateParameterName(relParam.Name));

                        execList.ManyToManyStatements.Add(Tuple.Create(sql, new List<QueryParam>{ propParm, relParam }));
                    }
                }
            }
        }

        internal UpdateSqlExecutionList Build()
        {
            var execList = new UpdateSqlExecutionList();
            var type = typeof(T);
            var store = new EntityAccessorStore();
            var accessor = store.GetEntityAccessor(type, Table);
            var dialect = session.SessionFactory.DbSettings.SqlDialect;
            LoadColumns(execList, Table, accessor);

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

                string whereExpression = wherebuilder.ToString();

                foreach (var stat in execList.Statements)
                {
                    stat.Value.WhereExpression = whereExpression;
                }
            }
            else throw new DataAccessException("Update missing where statement. Goliath cannot run an update without filters");

            return execList;
        }

        NonQueryFilterClause<T> CreateFilter(EntityMap map, SqlOperator preOperator, string propertyName)
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

        public IFilterNonQueryClause<T> Where(string propertyName)
        {
            return CreateFilter(Table, SqlOperator.AND, propertyName);
        }

        public IFilterNonQueryClause<T> Where<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return CreateFilter(Table, SqlOperator.AND, property.GetMemberName());
        }

        #endregion

        #region IBinaryNonQueryOperation<T> Members

        public IFilterNonQueryClause<T> And<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return CreateFilter(Table, SqlOperator.AND, property.GetMemberName());
        }

        public IFilterNonQueryClause<T> And(string propertyName)
        {
            return CreateFilter(Table, SqlOperator.AND, propertyName);
        }

        public IFilterNonQueryClause<T> Or<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return CreateFilter(Table, SqlOperator.OR, property.GetMemberName());
        }

        public IFilterNonQueryClause<T> Or(string propertyName)
        {
            return CreateFilter(Table, SqlOperator.OR, propertyName);
        }

        public int Execute()
        {
            var execList = Build();
            int total = 0;
            var runner = new SqlCommandRunner();
            foreach (var update in execList.Statements.Values)
            {

                var parameters = new List<QueryParam>();
                parameters.AddRange(whereParameters);
                parameters.AddRange(update.Columns.Values.Select(p => p.Item1));
                total += runner.ExecuteNonQuery(session, update.ToString(session.SessionFactory.DbSettings.SqlDialect), parameters.ToArray());

            }

            foreach (var manyToManyOp in execList.ManyToManyStatements)
            {
                total += runner.ExecuteNonQuery(session, manyToManyOp.Item1, manyToManyOp.Item2.ToArray());
            }

            return total;
        }

        #endregion
    }
}

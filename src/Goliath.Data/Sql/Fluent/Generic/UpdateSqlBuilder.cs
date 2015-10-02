using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Goliath.Data.Collections;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Entity;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    public class UpdateSqlBuilder<T> : NonQuerySqlBuilderBase<T>
    {
        static ILogger logger;

        static UpdateSqlBuilder()
        {
            logger = Logger.GetLogger(typeof(UpdateSqlBuilder<T>));
        }

        public UpdateSqlBuilder(ISession session, EntityMap entityMap, T entity) : base(session, entityMap, entity) { }

        public UpdateSqlBuilder(ISession session, T entity) : base(session, entity) { }

        void LoadColumns(UpdateSqlExecutionList execList, EntityMap entityMap, EntityAccessor accessor)
        {
            var updateBodyInfo = new UpdateSqlBodyInfo() { TableName = entityMap.TableName };

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
                        return;
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

            //Tuple<QueryParam, bool> tuple = Tuple.Create(new QueryParam(string.Format("{0}_{1}",entityMap.TableAlias, prop.ColumnName)) { Value = val }, isRel);
            Tuple<QueryParam, bool> tuple = Tuple.Create(QueryParam.CreateParameter(prop, string.Format("{0}_{1}", TableQueryMap.CreatePrefix(2,2), prop.ColumnName), val), isRel);
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

                    var relParam = new QueryParam(rel.MapReferenceColumn, rel.DbType) { Value = relPropInfo.GetMethod(item) };
                    var propParm = new QueryParam(rel.MapColumn, rel.DbType) { Value = propValue };
                    var dialect = session.SessionFactory.DbSettings.SqlDialect;
                    if (isInsertStatement)
                    {
                        var sql = string.Format("INSERT INTO {0} ({1}, {2}) VALUES({3},{4})",
                                                rel.MapTableName, rel.MapColumn, rel.MapReferenceColumn, dialect.CreateParameterName(propParm.Name), dialect.CreateParameterName(relParam.Name));

                        execList.ManyToManyStatements.Add(Tuple.Create(sql, new List<QueryParam> { propParm, relParam }));
                    }
                    else
                    {
                        var sql = string.Format("DELETE FROM {0} WHERE {1} = {3} AND {2} = {4}",
                                                rel.MapTableName, rel.MapColumn, rel.MapReferenceColumn, dialect.CreateParameterName(propParm.Name), dialect.CreateParameterName(relParam.Name));

                        execList.ManyToManyStatements.Add(Tuple.Create(sql, new List<QueryParam> { propParm, relParam }));
                    }
                }
            }
        }

        internal UpdateSqlExecutionList Build()
        {
            var execList = new UpdateSqlExecutionList();
            var store = new EntityAccessorStore();
            var accessor = store.GetEntityAccessor(entityType, Table);
            var dialect = session.SessionFactory.DbSettings.SqlDialect;
            LoadColumns(execList, Table, accessor);

            var whereExpression = BuildWhereExpression(dialect);

            foreach (var stat in execList.Statements)
            {
                stat.Value.WhereExpression = whereExpression;
            }

            return execList;
        }

        #region IBinaryNonQueryOperation<T> Members

        public override int Execute()
        {
            //if it's a trackable entity and no changes where made, let's not waste our time running a query
            var trackable = entity as ITrackable;
            if (trackable != null && !trackable.IsDirty)
                return 0;

            var execList = Build();
            int total = 0;
            var runner = new SqlCommandRunner();
            var dialect = session.SessionFactory.DbSettings.SqlDialect;
            //if(trackable != null)
            //{
            //    logger.Log(LogLevel.Debug, string.Format("changes found {0} ", trackable.ChangeTracker.GetChangedItems().Count));
            //}

            foreach (var update in execList.Statements.Values)
            {

                var parameters = new List<QueryParam>();
                parameters.AddRange(whereParameters);
                parameters.AddRange(update.Columns.Values.Select(p => p.Item1));
                total += runner.ExecuteNonQuery(session, update.ToString(dialect), parameters.ToArray());

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

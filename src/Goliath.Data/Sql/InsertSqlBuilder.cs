using System;
using Goliath.Data;
using Goliath.Data.Mapping;
using Goliath.Data.DataAccess;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class InsertSqlBuilder
    {
        readonly EntityAccessorStore entityAccessorStore = new EntityAccessorStore();

        /// <summary>
        /// Builds the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public InsertSqlExecutionList Build<T>(T entity, ISession session) where T : class
        {
            var entMap = session.SessionFactory.DbSettings.Map.GetEntityMap(typeof(T).FullName);
            return Build(entity, entMap, session);
        }

        /// <summary>
        /// Builds the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public InsertSqlExecutionList Build<T>(T entity, EntityMap entityMap, ISession session) where T : class
        {
            var executionList = new InsertSqlExecutionList();
            var entityType = typeof(T);
            Build(entity, entityType, entityMap, executionList, session);
            return executionList;
        }

        /// <summary>
        /// Builds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="executionList">The execution list.</param>
        /// <param name="session">The session.</param>
        /// <exception cref="System.ArgumentNullException">entity</exception>
        /// <exception cref="GoliathDataException">Property  + prop.Name +  not found in entity.</exception>
        public void Build(object entity, Type entityType, EntityMap entityMap, InsertSqlExecutionList executionList, ISession session)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (session == null)
                throw new ArgumentNullException("session");

            if (entityMap == null)
                throw new ArgumentNullException("entityMap");

            var info = new InsertSqlInfo { TableName = entityMap.TableName };
            var entityAccessor = entityAccessorStore.GetEntityAccessor(entityType, entityMap);
            var converterStore = session.SessionFactory.DbSettings.ConverterStore;

            Tuple<string, PropertyAccessor> pkTuple = null;
            if (entityMap.PrimaryKey != null)
            {
                pkTuple = ProcessPrimaryKey(entity, entityType, entityMap, info, entityAccessor, executionList, session);
            }

            foreach (var prop in entityMap)
            {
                if (prop.IsPrimaryKey)
                    continue;

                var rel = prop as Relation;
                if (rel != null)
                {
                    ProcessRelation(rel, entity, entityMap, info, entityAccessor, executionList, converterStore, session);
                }
                else
                {
                    var paramName = ParameterNameBuilderHelper.QueryParamName(entityMap, prop.ColumnName);
                    if (!info.Columns.ContainsKey(paramName))
                    {
                        PropertyAccessor pinf;
                        if (!entityAccessor.Properties.TryGetValue(prop.Name, out pinf))
                            throw new GoliathDataException("Property " + prop.Name + " not found in entity.");

                        info.Columns.Add(paramName, prop.ColumnName);
                        var propVal = pinf.GetMethod(entity);
                        info.Parameters.Add(paramName, new QueryParam(paramName, propVal));
                    }
                }
            }

            if ((pkTuple != null) && (pkTuple.Item2 != null))
            {
                var resultType = pkTuple.Item2.PropertyType;

                if (!info.DelayExecute)
                {
                    var pkValue = executionList.ExcuteStatement(session, info, resultType);
                    QueryParam pkQueryParam;
                    if (!info.Parameters.TryGetValue(pkTuple.Item1, out pkQueryParam))
                    {
                        pkQueryParam = new QueryParam(pkTuple.Item1);
                    }

                    pkQueryParam.Value = pkValue;
                    pkTuple.Item2.SetMethod(entity, pkValue);
                    executionList.GeneratedKeys.Add(pkTuple.Item1, pkQueryParam);
                }
                else
                {
                    executionList.ExcuteStatement(session, info, typeof(object));
                }
            }
            else
            {
                executionList.ExcuteStatement(session, info, typeof(object));
            }

            //check many to many relations and process them.
            foreach (var rel in entityMap.Relations)
            {
                if (rel.RelationType == RelationshipType.ManyToMany)
                {
                    ProcessManyToManyRelation(rel, entity, entityMap, info, entityAccessor, executionList, converterStore, session);
                }
            }

        }

        Tuple<string, PropertyAccessor> ProcessPrimaryKey(object entity, Type entityType, EntityMap entityMap, InsertSqlInfo info, EntityAccessor entityAccessor, InsertSqlExecutionList executionList, ISession session)
        {
            var dialect = session.SessionFactory.DbSettings.SqlDialect;
            var converterStore = session.SessionFactory.DbSettings.ConverterStore;
            PropertyAccessor pinf = null;
            string paramName = null;

            //TODO: remove support for multiple keys for one entity

            foreach (var pk in entityMap.PrimaryKey.Keys)
            {
                var rel = pk.Key as Relation;
                if (rel != null)
                {
                    var relEntMap = session.SessionFactory.DbSettings.Map.GetEntityMap(rel.ReferenceEntityName);
                    paramName = ParameterNameBuilderHelper.QueryParamName(relEntMap, rel.ReferenceColumn);
                    var pkQueryParam = new QueryParam(paramName);
                    info.Parameters.Add(paramName, pkQueryParam);
                    info.Columns.Add(paramName, pk.Key.ColumnName);
                    Build(entity, entityType, relEntMap, executionList, session);

                    continue;
                }

                SqlOperationPriority priority;
                paramName = ParameterNameBuilderHelper.QueryParamName(entityMap, pk.Key.ColumnName);
                if (!entityAccessor.Properties.TryGetValue(pk.Key.Name, out pinf))
                    throw new GoliathDataException("Property " + pk.Key.Name + " not found in entity.");

                if (!pk.KeyGenerator.IsDatabaseGenerated)
                {
                    info.DelayExecute = true;

                    var pkQueryParam = new QueryParam(paramName);

                    object pkValue;
                    if (HasUnsavedValue(pk, pinf, entity, converterStore, out pkValue))
                    {
                        var generatedKey = pk.KeyGenerator.GenerateKey(dialect, entityMap, pk.Key.Name, out priority);
                        pinf.SetMethod(entity, generatedKey);
                        pkQueryParam.Value = generatedKey;
                    }
                    else
                    {
                        pkQueryParam.Value = pkValue;
                    }

                    info.Parameters.Add(paramName, pkQueryParam);
                    info.Columns.Add(paramName, pk.Key.ColumnName);
                }
                else
                {
                    var keyGenSql = pk.KeyGenerator.GenerateKey(dialect, entityMap, pk.Key.Name, out priority) as string;

                    if (!string.IsNullOrWhiteSpace(keyGenSql))
                        info.DbKeyGenerateSql.Add(keyGenSql);
                }
            }

            return Tuple.Create(paramName, pinf);
        }

        bool HasUnsavedValue(PrimaryKeyProperty pk, PropertyAccessor pinf, object entity, ITypeConverterStore converterStore, out object pkValue)
        {
            if (!pk.UnsavedValueProcessed)
            {
                pk.UnsavedValue = pk.GetUnsavedValue(pinf.PropertyType, converterStore);
                pk.UnsavedValueProcessed = true;
            }

            pkValue = pinf.GetMethod(entity);

            var rlt = object.Equals(pkValue, pk.UnsavedValue);
            return rlt;
        }

        void ProcessManyToManyRelation(Relation rel, object entity, EntityMap entityMap, InsertSqlInfo info, EntityAccessor entityAccessor, InsertSqlExecutionList executionList, ITypeConverterStore converterStore, ISession session)
        {
            if ((rel.RelationType != RelationshipType.ManyToMany) || !rel.Inverse)
                return;

            PropertyAccessor pinf;
            if (!entityAccessor.Properties.TryGetValue(rel.Name, out pinf))
                throw new GoliathDataException("Property " + rel.Name + " not found in entity.");

            var collection = pinf.GetMethod(entity) as System.Collections.IEnumerable;
            int counter = 1;

            if (collection != null)
            {
                foreach (var relObj in collection)
                {
                    if (relObj == null)
                        continue;

                    var relEntityType = relObj.GetType();
                    var relEntityMap = session.SessionFactory.DbSettings.Map.GetEntityMap(relEntityType.FullName);
                    var relEntAccessor = entityAccessorStore.GetEntityAccessor(relEntityType, relEntityMap);
                    var paramName = ParameterNameBuilderHelper.QueryParamName(entityMap, rel.MapColumn + counter);
                    var mapParamName = ParameterNameBuilderHelper.QueryParamName(entityMap, rel.MapReferenceColumn + counter);

                    PropertyAccessor relPinf;
                    if (!relEntAccessor.Properties.TryGetValue(rel.ReferenceProperty, out relPinf))
                        throw new GoliathDataException("Reference " + rel.ReferenceProperty + " not found in entity.");

                    if (relEntityMap.PrimaryKey != null)
                    {
                        foreach (var pk in relEntityMap.PrimaryKey.Keys)
                        {
                            object pkValue;
                            if (HasUnsavedValue(pk, relPinf, relObj, converterStore, out pkValue))
                            {
                                Build(relObj, relEntityType, relEntityMap, executionList, session);
                            }

                            var manyToManyInfo = new InsertSqlInfo { DelayExecute = true, TableName = rel.MapTableName };

                            var param1 = new QueryParam(mapParamName) { Value = relPinf.GetMethod(relObj) };
                            manyToManyInfo.Columns.Add(mapParamName, rel.MapReferenceColumn);
                            manyToManyInfo.Parameters.Add(mapParamName, param1);

                            PropertyAccessor mappedPinf;
                            if (!entityAccessor.Properties.TryGetValue(rel.MapPropertyName, out mappedPinf))
                                throw new GoliathDataException("Property " + rel.MapPropertyName + " not found in entity" + entityMap.FullName + ".");

                            var param2 = new QueryParam(paramName) { Value = mappedPinf.GetMethod(entity) };
                            manyToManyInfo.Columns.Add(paramName, rel.ReferenceColumn);
                            manyToManyInfo.Parameters.Add(paramName, param2);

                            executionList.ExcuteStatement(session, manyToManyInfo, typeof(object));
                        }
                    }

                    counter++;
                }
            }
        }

        void ProcessRelation(Relation rel, object entity, EntityMap entityMap, InsertSqlInfo info, EntityAccessor entityAccessor, InsertSqlExecutionList executionList, ITypeConverterStore converterStore, ISession session)
        {
            if ((rel.RelationType != RelationshipType.ManyToOne) || rel.IsAutoGenerated)
                return;

            var paramName = ParameterNameBuilderHelper.QueryParamName(entityMap, rel.ColumnName);
            //first let's see if property is null or not  than we need to compare the primary key with unsaved value
            PropertyAccessor pinf;
            if (!entityAccessor.Properties.TryGetValue(rel.Name, out pinf))
                throw new GoliathDataException("Property " + rel.Name + " not found in entity.");

            object relValue = pinf.GetMethod(entity);
            if (relValue != null)
            {
                //now we need to get unsaved value;
                var relEntityType = relValue.GetType();
                var relEntityMap = session.SessionFactory.DbSettings.Map.GetEntityMap(relEntityType.FullName);
                var relEntAccessor = entityAccessorStore.GetEntityAccessor(relEntityType, relEntityMap);

                PropertyAccessor relPinf;
                if (!relEntAccessor.Properties.TryGetValue(rel.ReferenceProperty, out relPinf))
                    throw new GoliathDataException("Reference " + rel.ReferenceProperty + " not found in entity.");

                if (relEntityMap.PrimaryKey != null)
                {
                    foreach (var pk in relEntityMap.PrimaryKey.Keys)
                    {
                        object pkValue;
                        if (HasUnsavedValue(pk, relPinf, relValue, converterStore, out pkValue))
                        {
                            Build(relValue, relEntityType, relEntityMap, executionList, session);
                        }

                        QueryParam qParam;
                        if (!info.Parameters.TryGetValue(paramName, out qParam))
                        {
                            qParam = new QueryParam(paramName);
                            info.Columns.Add(paramName, rel.ColumnName);
                            info.Parameters.Add(paramName, qParam);
                        }

                        qParam.Value = relPinf.GetMethod(relValue);
                    }
                }
            }
        }

    }
}

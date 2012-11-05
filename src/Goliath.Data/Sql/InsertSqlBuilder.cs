using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="entityMap">The entity map.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public InsertSqlExecutionList Build<T>(T entity, EntityMap entityMap, ISession session) where T : class
        {
            var executionList = new InsertSqlExecutionList();
            Type entityType = typeof(T);
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

            var dialect = session.SessionFactory.DbSettings.SqlDialect;
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
                    ProcessRelation(rel, entity, entityType, entityMap, info, entityAccessor, executionList, converterStore, session);
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
                var pkValue = executionList.ExcuteStatement(session, info, resultType);

                if (!info.DelayExecute)
                {
                    QueryParam pkQueryParam;
                    if (!info.Parameters.TryGetValue(pkTuple.Item1, out pkQueryParam))
                    {
                        pkQueryParam = new QueryParam(pkTuple.Item1);
                    }

                    pkQueryParam.Value = pkValue;
                    pkTuple.Item2.SetMethod(entity, pkValue);
                    executionList.GeneratedKeys.Add(pkTuple.Item1, pkQueryParam);
                }
            }
            else
            {
                executionList.ExcuteStatement(session, info, typeof(object));
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

        void ProcessRelation(Relation rel, object entity, Type entityType, EntityMap entityMap, InsertSqlInfo info, EntityAccessor entityAccessor, InsertSqlExecutionList executionList, ITypeConverterStore converterStore, ISession session)
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
                var relEntityMap = session.SessionFactory.DbSettings.Map.GetEntityMap(rel.ReferenceEntityName);
                var relEntityType = relValue.GetType();
                var relEntAccessor = entityAccessorStore.GetEntityAccessor(relEntityType, relEntityMap);

                PropertyAccessor relPinf;
                if (!entityAccessor.Properties.TryGetValue(rel.ReferenceProperty, out relPinf))
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

    /// <summary>
    /// 
    /// </summary>
    public class InsertSqlExecutionList
    {
        readonly List<InsertSqlInfo> statements = new List<InsertSqlInfo>();
        readonly Dictionary<string, QueryParam> generatedKeys = new Dictionary<string, QueryParam>();

        /// <summary>
        /// Gets the statements.
        /// </summary>
        /// <value>
        /// The statements.
        /// </value>
        public List<InsertSqlInfo> Statements { get { return statements; } }

        /// <summary>
        /// Gets the generated keys.
        /// </summary>
        /// <value>
        /// The generated keys.
        /// </value>
        public Dictionary<string, QueryParam> GeneratedKeys
        {
            get { return generatedKeys; }
        }

        /// <summary>
        /// Excutes the statement.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="statement">The statement.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <returns></returns>
        internal object ExcuteStatement(ISession session, InsertSqlInfo statement, Type resultType)
        {
            object value = null;

            if (!statement.DelayExecute)
            {
                //execute all
                var sql = statement.ToString(session.SessionFactory.DbSettings.SqlDialect);
                var cmdr = new SqlCommandRunner();
                var parameters = new List<QueryParam>();
                parameters.AddRange(GeneratedKeys.Values);

                foreach (var queryParam in statement.Parameters)
                {
                    if (!GeneratedKeys.ContainsKey(queryParam.Key))
                        parameters.Add(queryParam.Value);
                }

                value = cmdr.ExecuteScalar(session, sql, resultType, parameters.ToArray());
                statement.Processed = true;
            }

            Statements.Add(statement);

            return value;
        }

        public void Execute(ISession session)
        {
            //TODO: execute all statements here
        }
    }
}

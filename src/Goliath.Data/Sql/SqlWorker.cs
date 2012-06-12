﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace Goliath.Data.Sql
{
    using DataAccess;
    using Providers;
    using Diagnostics;
    using Mapping;

    /// <summary>
    /// 
    /// </summary>
    public sealed class SqlWorker : ISqlWorker
    {
        SqlMapper sqlMapper;
        GetSetStore getSetStore;
        static ILogger logger;
        ITypeConverterStore typeConverterStore;

        static SqlWorker()
        {
            logger = Logger.GetLogger(typeof(SqlWorker));
        }

        internal SqlWorker(SqlMapper sqlMapper, GetSetStore getSetStore, ITypeConverterStore typeConverterStore)
        {
            if (sqlMapper == null)
                throw new ArgumentNullException("sqlMapper");
            if (getSetStore == null)
                throw new ArgumentNullException("getSetStore");

            this.sqlMapper = sqlMapper;
            this.getSetStore = getSetStore;
            this.typeConverterStore = typeConverterStore;
        }

        internal static SelectSqlBuilder BuildSelectSql(EntityMap entityMap, SqlMapper sqlMapper, IDbAccess dataAccess, PropertyQueryParam[] filters)
        {
            SelectSqlBuilder queryBuilder = new SelectSqlBuilder(sqlMapper, entityMap);
            if ((filters != null) && (filters.Length > 0))
            {
                for (int i = 0; i < filters.Length; i++)
                {
                    var prop = entityMap[filters[i].PropertyName];
                    if (prop == null)
                        throw new MappingException(string.Format("Property {0} not found in mapped entity {1}", filters[i].PropertyName, entityMap.FullName));

                    filters[i].SetParameterName(prop.ColumnName, entityMap.TableAlias);
                    WhereStatement w = new WhereStatement(prop.ColumnName)
                    {
                        Operator = filters[i].ComparisonOperator,
                        PostOperator = filters[i].PostOperator,
                        RightOperand = new StringOperand(sqlMapper.CreateParameterName(ParameterNameBuilderHelper.ColumnQueryName(prop.ColumnName, entityMap.TableAlias)))
                    };

                    queryBuilder.Where(w);
                }
            }

            return queryBuilder;
        }

        #region Inserts

        internal Dictionary<string, KeyGenOperationInfo> GeneratePksForInsert(object entity, EntityMap entityMap, EntityGetSetInfo getSetInfo)
        {
            Dictionary<string, KeyGenOperationInfo> keygenerationOperations = new Dictionary<string, KeyGenOperationInfo>();

            if (entityMap.PrimaryKey != null)
            {
                foreach (var pk in entityMap.PrimaryKey.Keys)
                {
                    if (pk.KeyGenerator == null)
                    {
                        continue;
                        //throw new MappingException(string.Format("No key generator specified for {0} for mapped entity {1}", pk.Key.PropertyName, entityMap.FullName));
                    }

                    if (!pk.Key.IsAutoGenerated)
                    {
                        PropInfo pInfo;
                        if (getSetInfo.Properties.TryGetValue(pk.Key.PropertyName, out pInfo) && pk.CanGenerateKey(pInfo, entity, typeConverterStore))
                        {
                            SqlOperationPriority priority;
                            var id = pk.KeyGenerator.GenerateKey(sqlMapper, entityMap, pk.Key.PropertyName, out priority);
                            pInfo.Setter(entity, id);
                        }
                    }
                    else
                    {
                        SqlOperationPriority priority;
                        string genText = pk.KeyGenerator.GenerateKey(sqlMapper, entityMap, pk.Key.PropertyName, out priority).ToString();
                        SqlOperationInfo genOper = new SqlOperationInfo() { CommandType = SqlStatementType.Select, Parameters = new QueryParam[] { }, SqlText = genText };
                        var genParamName = ParameterNameBuilderHelper.ColumnQueryName(pk.Key.ColumnName, entityMap.TableAlias);

                        PropInfo pInfo;
                        if (getSetInfo.Properties.TryGetValue(pk.Key.PropertyName, out pInfo))
                        {
                            KeyGenOperationInfo genKeyOper = new KeyGenOperationInfo()
                            {
                                Entity = entity,
                                PropertyName = pk.Key.PropertyName,
                                PropertyType = pInfo.PropertType,
                                Operation = genOper,
                                Priority = priority
                            };

                            keygenerationOperations.Add(genParamName, genKeyOper);
                        }
                    }
                }
            }

            return keygenerationOperations;
        }

        /// <summary>
        /// Builds the insert SQL.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public BatchSqlOperation BuildInsertSql<TEntity>(EntityMap entityMap, TEntity entity, bool recursive)
        {
            BatchSqlOperation operation = new BatchSqlOperation() { Priority = SqlOperationPriority.Medium };
            //Dictionary<string, PropertyQueryParam> neededParams = new Dictionary<string, PropertyQueryParam>();
            BuildInsertSql(entity, entityMap, typeof(TEntity), null, null, null, operation, recursive, 0, -1);

            return operation;
        }

        void BuildInsertSql
        (
            object entity,
            EntityMap entityMap,
            Type entityType,

            object parentEntity,
            EntityMap parentEntityMap,
            Type parentEntityType,
            BatchSqlOperation batchOperation,

            bool recursive,
            int recursionLevel = 0,
            int rootRecursionLevel = 0
         )
        {
            EntityMap baseEntMap = null;
            Dictionary<string, KeyGenOperationInfo> keygenerationOperations = new Dictionary<string, KeyGenOperationInfo>();
            bool isSubclass = entityMap.IsSubClass;            
            InsertSqlBuilder baseInsertSqlBuilder = null;
            BatchSqlOperation operation = null;

            EntityGetSetInfo entGetSets = getSetStore.GetReflectionInfoAddIfMissing(entityType, entityMap);

            if (isSubclass)
            {
                //get base class first
                baseEntMap = entityMap.Parent.GetEntityMap(entityMap.Extends);
                baseInsertSqlBuilder = new InsertSqlBuilder(sqlMapper, baseEntMap, recursionLevel, rootRecursionLevel);
                keygenerationOperations = GeneratePksForInsert(entity, baseEntMap, entGetSets);

                var baseParamDictionary = InsertSqlBuilder.BuildInsertQueryParams(entity, entGetSets, baseEntMap, getSetStore, recursionLevel, rootRecursionLevel);
                SqlOperationInfo baseClassOperation = new SqlOperationInfo() { CommandType = SqlStatementType.Insert };
                baseClassOperation.SqlText = baseInsertSqlBuilder.ToSqlString();
                List<QueryParam> baseParameters = new List<QueryParam>();
                baseParameters.AddRange(baseParamDictionary.Values);
                baseClassOperation.Parameters = baseParameters;
                operation = new BatchSqlOperation() { Priority = SqlOperationPriority.Low };

                batchOperation.Operations.Add(baseClassOperation);
                batchOperation.Priority = SqlOperationPriority.Medium;
                batchOperation.SubOperations.Add(operation);
            }
            else
            {
                keygenerationOperations = GeneratePksForInsert(entity, entityMap, entGetSets);
                operation = batchOperation;
                operation.Priority = SqlOperationPriority.Medium;
            }

            InsertSqlBuilder entInsertSqlBuilder = new InsertSqlBuilder(sqlMapper, entityMap, recursionLevel, rootRecursionLevel);

            var paramDictionary = InsertSqlBuilder.BuildInsertQueryParams(entity, entGetSets, entityMap, getSetStore, recursionLevel, rootRecursionLevel);
            SqlOperationInfo operationInfo = new SqlOperationInfo() { CommandType = SqlStatementType.Insert };
            operationInfo.SqlText = entInsertSqlBuilder.ToSqlString();
            List<QueryParam> parameters = new List<QueryParam>();
            parameters.AddRange(paramDictionary.Values);
            operationInfo.Parameters = parameters;
            operation.Operations.Add(operationInfo);

            if (keygenerationOperations.Count > 0)
            {

                if (isSubclass)
                {
                    batchOperation.KeyGenerationOperations = keygenerationOperations;
                    batchOperation.Priority = SqlOperationPriority.High;
                }
                else
                {
                    operation.KeyGenerationOperations = keygenerationOperations;
                    operation.Priority = SqlOperationPriority.High;
                }
            }

            if (recursive)
            {
                rootRecursionLevel++;
                foreach (var rel in entityMap.Relations)
                {
                    if (rel.RelationType == RelationshipType.ManyToOne)
                        continue;

                    else if (rel.RelationType == RelationshipType.OneToMany)
                    {
                        PropInfo pInfo;
                        if (entGetSets.Properties.TryGetValue(rel.PropertyName, out pInfo))
                        {
                            var colGetter = pInfo.Getter(entity);

                            if ((colGetter != null) && (colGetter is System.Collections.IEnumerable))
                            {
                                var list = (System.Collections.IEnumerable)colGetter;
                                foreach (var o in list)
                                {
                                    if (o == null)
                                        continue;
                                    //get type
                                    var reltype = o.GetType();
                                    //get map
                                    var relmap = entityMap.Parent.GetEntityMap(reltype.FullName);
                                    BatchSqlOperation relOper = new BatchSqlOperation() { Priority = SqlOperationPriority.Low };
                                    operation.SubOperations.Add(relOper);
                                    BuildInsertSql(o, relmap, reltype, entity, entityMap, entityType, relOper, true, ++recursionLevel, rootRecursionLevel);
                                }
                            }
                        }
                    }
                    else if ((rel.RelationType == RelationshipType.ManyToMany) && rel.Inverse)
                    {
                        PropInfo pInfo;
                        if (entGetSets.Properties.TryGetValue(rel.PropertyName, out pInfo))
                        {
                            var colGetter = pInfo.Getter(entity);
                            if ((colGetter != null) && (colGetter is System.Collections.IEnumerable))
                            {
                                var list = (System.Collections.IEnumerable)colGetter;
                                foreach (var mappedObject in list)
                                {
                                    if (mappedObject == null)
                                        continue;

                                    var reltype = mappedObject.GetType();
                                    var relMap = entityMap.Parent.GetEntityMap(reltype.FullName);
                                    //build insert statement
                                    SqlOperationInfo manyToManyOp = new SqlOperationInfo();
                                    Property mapRel = relMap.GetProperty(rel.ReferenceProperty);

                                    if (mapRel != null)
                                    {
                                        var paramName1 = InsertSqlBuilder.BuildParameterNameWithLevel(rel.MapColumn, entityMap.TableAlias, recursionLevel);
                                        var paramName2 = InsertSqlBuilder.BuildParameterNameWithLevel(rel.MapReferenceColumn, relMap.TableAlias, recursionLevel);
                                        manyToManyOp.SqlText = string.Format("INSERT INTO {0} ({1}, {2}) VALUES({3},{4})", rel.MapTableName, rel.MapColumn, rel.MapReferenceColumn, sqlMapper.CreateParameterName(paramName1), sqlMapper.CreateParameterName(paramName2));
                                        var param1Prop = entGetSets.Properties[mapRel.PropertyName];

                                        EntityGetSetInfo mappedGetSet = getSetStore.GetReflectionInfoAddIfMissing(reltype, relMap);

                                        var param2Prop = mappedGetSet.Properties[rel.ReferenceProperty];
                                        manyToManyOp.Parameters = new ParamHolder[] { new ParamHolder(paramName1, param1Prop.Getter, entity) { IsNullable = rel.IsNullable }, 
                                            new ParamHolder(paramName2, param2Prop.Getter, mappedObject) { IsNullable = mapRel.IsNullable } };

                                        var manyToManySubOp = new BatchSqlOperation() { Priority = SqlOperationPriority.Low, KeyGenerationOperations = new Dictionary<string, KeyGenOperationInfo>() };
                                        manyToManySubOp.Operations.Add(manyToManyOp);
                                        operation.SubOperations.Add(manyToManySubOp);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Builds the update SQL.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="updateManyToManyRelation">if set to <c>true</c> [update many to many relation].</param>
        /// <returns></returns>
        public BatchSqlOperation BuildUpdateSql<TEntity>(EntityMap entityMap, TEntity entity, bool updateManyToManyRelation = false)
        {
            BatchSqlOperation operation = new BatchSqlOperation() { Priority = SqlOperationPriority.Medium };

            BuildUpdateSql(entity, entityMap, typeof(TEntity), null, null, null, operation, updateManyToManyRelation, 0, 0);

            return operation;

        }

        void BuildUpdateSql
       (
           object entity,
           EntityMap entityMap,
           Type entityType,

           object parentEntity,
           EntityMap parentEntityMap,
           Type parentEntityType,
           BatchSqlOperation batchOperation,

           bool recursive,
           int recursionLevel = 0,
           int rootRecursionLevel = 0
        )
        {
            EntityMap baseEntMap = null;           
            UpdateSqlBuilder baseUpdateBuilder = null;
            BatchSqlOperation operation = null;
            bool isSubclass = entityMap.IsSubClass;

            EntityGetSetInfo entGetSets = getSetStore.GetReflectionInfoAddIfMissing(entityType, entityMap);

            if (isSubclass)
            {
                baseEntMap = entityMap.Parent.GetEntityMap(entityMap.Extends);
                baseUpdateBuilder = new UpdateSqlBuilder(sqlMapper, baseEntMap, recursionLevel, rootRecursionLevel);

                var baseParamDictionary = UpdateSqlBuilder.BuildUpdateQueryParams(entity, entGetSets, baseEntMap, getSetStore, recursionLevel, rootRecursionLevel);
                SqlOperationInfo baseSqlOp = new SqlOperationInfo() { CommandType = SqlStatementType.Update };

                var whereCollection = UpdateSqlBuilder.BuildWhereStatementFromPrimaryKey(baseEntMap, sqlMapper, recursionLevel);
                baseSqlOp.SqlText = baseUpdateBuilder.Where(whereCollection).ToSqlString();
                List<QueryParam> baseParameters = new List<QueryParam>();
                baseParameters.AddRange(baseParamDictionary.Values);
                baseSqlOp.Parameters = baseParameters;
                operation = new BatchSqlOperation() { Priority = SqlOperationPriority.Low };

                batchOperation.Operations.Add(baseSqlOp);
                batchOperation.Priority = SqlOperationPriority.Medium;
                batchOperation.SubOperations.Add(operation);
            }
            else
            {
                operation = batchOperation;
                operation.Priority = SqlOperationPriority.Medium;
            }

            var wheres = UpdateSqlBuilder.BuildWhereStatementFromPrimaryKey(entityMap, sqlMapper, recursionLevel);
            UpdateSqlBuilder entUpdateSqlBuilder = new UpdateSqlBuilder(sqlMapper, entityMap, recursionLevel, rootRecursionLevel);
            var paramDictionary = UpdateSqlBuilder.BuildUpdateQueryParams(entity, entGetSets, entityMap, getSetStore, recursionLevel, rootRecursionLevel);
            SqlOperationInfo operationInfo = new SqlOperationInfo { CommandType = SqlStatementType.Update };
            operationInfo.SqlText = entUpdateSqlBuilder.Where(wheres).ToSqlString();

            List<QueryParam> parameters = new List<QueryParam>();
            parameters.AddRange(paramDictionary.Values);
            operationInfo.Parameters = parameters;
            operation.Operations.Add(operationInfo);

        }

        /// <summary>
        /// Builds the delete SQL.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="cascade">if set to <c>true</c> cascade delete.</param>
        /// <returns></returns>
        public BatchSqlOperation BuildDeleteSql<TEntity>(EntityMap entityMap, TEntity entity, bool cascade)
        {
            BatchSqlOperation operation = new BatchSqlOperation() { Priority = SqlOperationPriority.Medium };
            BuildDeleteSql(entity, entityMap, typeof(TEntity), operation);
            return operation;
        }

        void BuildDeleteSql
       (
           object entity,
           EntityMap entityMap,
           Type entityType,
           BatchSqlOperation batchOperation
        )
        {
            EntityMap baseEntMap = null;
            EntityGetSetInfo entGetSets = getSetStore.GetReflectionInfoAddIfMissing(entityType, entityMap);
 
            bool isSubclass = entityMap.IsSubClass;

            var wheres = UpdateSqlBuilder.BuildWhereStatementFromPrimaryKey(entityMap, sqlMapper, 0);
            DeleteSqlBuilder entDeleteSqlBuilder = new DeleteSqlBuilder(sqlMapper, entityMap);
            var paramDictionary = DeleteSqlBuilder.BuildDeleteQueryParams(entity, entGetSets, entityMap, getSetStore);
            SqlOperationInfo operationInfo = new SqlOperationInfo { CommandType = SqlStatementType.Delete };
            operationInfo.SqlText = entDeleteSqlBuilder.Where(wheres).ToSqlString();

            List<QueryParam> parameters = new List<QueryParam>();
            parameters.AddRange(paramDictionary.Values);
            operationInfo.Parameters = parameters;
            batchOperation.Operations.Add(operationInfo);


            if (isSubclass)
            {
                baseEntMap = entityMap.Parent.GetEntityMap(entityMap.Extends);
                var baseDeleteBuilder = new DeleteSqlBuilder(sqlMapper, baseEntMap);

                var baseParamDictionary = DeleteSqlBuilder.BuildDeleteQueryParams(entity, entGetSets, baseEntMap, getSetStore);
                SqlOperationInfo baseSqlOp = new SqlOperationInfo() { CommandType = SqlStatementType.Delete };

                var whereCollection = UpdateSqlBuilder.BuildWhereStatementFromPrimaryKey(baseEntMap, sqlMapper, 0);
                baseSqlOp.SqlText = baseDeleteBuilder.Where(whereCollection).ToSqlString();
                List<QueryParam> baseParameters = new List<QueryParam>();
                baseParameters.AddRange(baseParamDictionary.Values);
                baseSqlOp.Parameters = baseParameters;
               var operation = new BatchSqlOperation() { Priority = SqlOperationPriority.Low };

               operation.Operations.Add(baseSqlOp);
                batchOperation.Priority = SqlOperationPriority.Medium;
                batchOperation.SubOperations.Add(operation);
            }
        }
    }
}

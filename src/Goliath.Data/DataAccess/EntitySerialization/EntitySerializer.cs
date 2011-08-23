using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Goliath.Data.Diagnostics;
using Goliath.Data.DynamicProxy;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Sql;

namespace Goliath.Data.DataAccess
{
    //TODO make this internal class
    //TODO use singleton for entitySerializer;
    /// <summary>
    /// 
    /// </summary>
    public class EntitySerializer : IEntitySerializer
    {

        static ConcurrentDictionary<Type, Delegate> factoryList = new ConcurrentDictionary<Type, Delegate>();
        static ILogger logger;
        internal ITypeConverterStore TypeConverterStore { get; set; }

        /// <summary>
        /// Gets the SQL mapper.
        /// </summary>
        /// <value></value>
        public SqlMapper SqlMapper { get; internal set; }

        //DbAccess dbAccess;

        static EntitySerializer()
        {
            logger = Logger.GetLogger(typeof(EntitySerializer));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializer"/> class.
        /// </summary>
        public EntitySerializer(SqlMapper sqlMapper) : this(sqlMapper, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializer"/> class.
        /// </summary>
        /// <param name="sqlMapper">The SQL mapper.</param>
        /// <param name="typeConverterStore">The type converter.</param>
        public EntitySerializer(SqlMapper sqlMapper, ITypeConverterStore typeConverterStore)
        {
            if (sqlMapper == null)
                throw new ArgumentNullException("sqlMapper");

            if (typeConverterStore == null)
                typeConverterStore = new TypeConverterStore();

            this.SqlMapper = sqlMapper;
            //this.dbAccess = dbAccess;
            this.TypeConverterStore = typeConverterStore;
        }

        #region IEntitySerializerFactory Members

        /// <summary>
        /// Registers the data hydrator.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        public void RegisterDataHydrator<TEntity>(Func<DbDataReader, EntityMap, TEntity> factoryMethod)
        {
            factoryList.TryAdd(typeof(TEntity), factoryMethod);
        }

        /// <summary>
        /// Serializes all.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        public IList<TEntity> SerializeAll<TEntity>(DbDataReader dataReader, EntityMap entityMap)
        {
            Delegate dlgMethod;
            Type type = typeof(TEntity);
            Func<DbDataReader, EntityMap, IList<TEntity>> factoryMethod = null;
            if (factoryList.TryGetValue(type, out dlgMethod))
            {
                factoryMethod = dlgMethod as Func<DbDataReader, EntityMap, IList<TEntity>>;
                if (factoryMethod == null)
                {
                    throw new GoliathDataException("unknown factory method");
                }
            }
            else
            {
                factoryMethod = CreateSerializerMethod<TEntity>(entityMap);
                factoryList.TryAdd(type, factoryMethod);
            }

            IList<TEntity> entityList = factoryMethod.Invoke(dataReader, entityMap);
            return entityList;
        }

        /// <summary>
        /// Hydrates the specified instance to hydrate.
        /// </summary>
        /// <param name="instanceToHydrate">The instance to hydrate.</param>
        /// <param name="typeOfInstance">The type of instance.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="dataReader">The data reader.</param>
        public void Hydrate(object instanceToHydrate, Type typeOfInstance, EntityMap entityMap, DbDataReader dataReader)
        {
            if (dataReader.HasRows)
            {
                Dictionary<string, int> columns = GetColumnNames(dataReader, entityMap.TableAlias);
                EntityGetSetInfo getSetInfo;

                if (!getSetStore.TryGetValue(typeOfInstance, out getSetInfo))
                {
                    getSetInfo = new EntityGetSetInfo(typeOfInstance);
                    getSetInfo.Load(entityMap);
                    getSetStore.Add(typeOfInstance, getSetInfo);
                }

                dataReader.Read();
                SerializeSingle(instanceToHydrate, typeOfInstance, entityMap, getSetInfo, columns, dataReader);
            }
        }

        /// <summary>
        /// Deserializes the specified key generator.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public BatchSqlOperation BuildInsertSql<TEntity>(EntityMap entityMap, TEntity entity, bool recursive)
        {

            BatchSqlOperation operation = new BatchSqlOperation() { Priority = SqlOperationPriority.Medium };
            Dictionary<string, PropertyQueryParam> neededParams = new Dictionary<string, PropertyQueryParam>();
            BuildInsertSql(entity, entityMap, typeof(TEntity), null, null, null, operation, neededParams, recursive);

            return operation;
        }

        //TODO: recursively extract insert for relationships for one-to-many relations.
        void BuildInsertSql
        (
            object entity,
            EntityMap entityMap,
            Type entityType,

            object parentEntity,
            EntityMap parentEntityMap,
            Type parentEntityType,

            BatchSqlOperation operation,
            Dictionary<string, PropertyQueryParam> neededParams,
            bool recursive
         )
        {
            InsertSqlBuilder sqlBuilder = new InsertSqlBuilder(SqlMapper, entityMap);

            EntityGetSetInfo getSetInfo;

            if (!getSetStore.TryGetValue(entityType, out getSetInfo))
            {
                getSetInfo = new EntityGetSetInfo(entityType);
                getSetInfo.Load(entityMap);
                getSetStore.Add(entityType, getSetInfo);
            }

            //bool keyWasGenerated = false;

            Dictionary<string, KeyGenOperationInfo> keygenerationOperations = new Dictionary<string, KeyGenOperationInfo>();

            if (entityMap.PrimaryKey != null)
            {
                foreach (var pk in entityMap.PrimaryKey.Keys)
                {
                    if (pk.KeyGenerator == null)
                        throw new MappingException(string.Format("No key generator specified for {0} for mapped entity {1}", pk.Key.PropertyName, entityMap.FullName));
                    if (!pk.Key.IsAutoGenerated)
                    {
                        PropInfo pInfo;
                        if (getSetInfo.Properties.TryGetValue(pk.Key.PropertyName, out pInfo))
                        {
                            SqlOperationPriority priority;
                            var id = pk.KeyGenerator.GenerateKey(entityMap, pk.Key.PropertyName, out priority);
                            pInfo.Setter(entity, id);
                            //keyWasGenerated = true;
                        }
                    }
                    else
                    {
                        SqlOperationPriority priority;
                        string genText = pk.KeyGenerator.GenerateKey(entityMap, pk.Key.PropertyName, out priority).ToString();
                        SqlOperationInfo genOper = new SqlOperationInfo() { CommandType = SqlStatementType.Select, Parameters = new QueryParam[] { }, SqlText = genText };
                        var genParamName = ParameterNameBuilderHelper.ColumnQueryName(pk.Key.ColumnName, entityMap.TableAlias);
                        KeyGenOperationInfo genKeyOper = new KeyGenOperationInfo() { Operation = genOper, Priority = priority };
                        keygenerationOperations.Add(genParamName, genKeyOper);
                    }
                }
            }

            var paramDictionary = InsertSqlBuilder.BuildQueryParams(entity, getSetInfo, entityMap, getSetStore);
            SqlOperationInfo qInfo = new SqlOperationInfo() { CommandType = SqlStatementType.Insert };
            qInfo.SqlText = sqlBuilder.ToSqlString();
            qInfo.Parameters = paramDictionary.Values;

            operation.Operations.Add(qInfo);

            if (keygenerationOperations.Count > 0)
            {
                operation.KeyGenerationOperations = keygenerationOperations;
                operation.Priority = SqlOperationPriority.High;
            }

            if (recursive)
            {
                foreach (var rel in entityMap.Relations)
                {
                    if (rel.RelationType == RelationshipType.ManyToOne)
                        continue;

                    else if (rel.RelationType == RelationshipType.OneToMany)
                    {
                        PropInfo pInfo;

                        if (getSetInfo.Properties.TryGetValue(rel.PropertyName, out pInfo))
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
                                    BuildInsertSql(o, relmap, reltype, entity, entityMap, entityType, relOper, neededParams, true);
                                }
                            }
                        }
                    }

                    else if (rel.RelationType == RelationshipType.ManyToMany)
                    {
                    }
                }
            }
        }

        GetSetStore getSetStore = new GetSetStore();

        #endregion

        Func<DbDataReader, EntityMap, IList<TEntity>> CreateSerializerMethod<TEntity>(EntityMap entityMap)
        {
            Func<DbDataReader, EntityMap, IList<TEntity>> func = (dbReader, entMap) =>
            {
                List<TEntity> list = new List<TEntity>();

                Type type = typeof(TEntity);
                Dictionary<string, int> columns = GetColumnNames(dbReader, entMap.TableAlias);

                EntityGetSetInfo getSetInfo;

                if (!getSetStore.TryGetValue(type, out getSetInfo))
                {
                    getSetInfo = new EntityGetSetInfo(type);
                    getSetInfo.Load(entMap);
                    getSetStore.Add(type, getSetInfo);
                }

                while (dbReader.Read())
                {
                    var instanceEntity = Activator.CreateInstance(type);
                    SerializeSingle(instanceEntity, type, entityMap, getSetInfo, columns, dbReader);
                    list.Add((TEntity)instanceEntity);
                }

                return list;
            };

            return func;
        }

        internal static Dictionary<string, int> GetColumnNames(DbDataReader dbReader, string tableAbbreviation)
        {
            Dictionary<string, int> columns = new Dictionary<string, int>();
            for (int i = 0; i < dbReader.FieldCount; i++)
            {
                var fieldName = dbReader.GetName(i);
                string tabb = string.Format("{0}_", tableAbbreviation);
                if (fieldName.StartsWith(tabb))
                {
                    var colName = ParameterNameBuilderHelper.GetPropNameFromQueryName(fieldName, tableAbbreviation);
                    columns.Add(colName, i);
                }
            }
            return columns;
        }

        internal void SerializeSingle(object instanceEntity, Type type, EntityMap entityMap, EntityGetSetInfo getSetInfo, Dictionary<string, int> columns, DbDataReader dbReader)
        {
            foreach (var keyVal in getSetInfo.Properties)
            {
                var prop = entityMap[keyVal.Key];
                int ordinal;
                if (prop != null)
                {
                    if (prop is Relation)
                    {
                        logger.Log(LogType.Info, string.Format("Read {0} is a relation", keyVal.Key));
                        Relation rel = (Relation)prop;
                        switch (rel.RelationType)
                        {
                            case RelationshipType.ManyToOne:
                                SerializeManyToOne manyToOneHelper = new SerializeManyToOne(SqlMapper, getSetStore);
                                manyToOneHelper.Serialize(this, rel, instanceEntity, keyVal.Value, entityMap, getSetInfo, columns, dbReader);
                                logger.Log(LogType.Info, string.Format("\t\t{0} is a ManyToOne", keyVal.Key));
                                break;
                            case RelationshipType.OneToMany:
                                SerializeOneToMany oneToManyHelper = new SerializeOneToMany(SqlMapper, getSetStore);
                                oneToManyHelper.Serialize(this, rel, instanceEntity, keyVal.Value, entityMap, getSetInfo, columns, dbReader);
                                logger.Log(LogType.Info, string.Format("\t\t{0} is a OneToMany", keyVal.Key));
                                break;
                            case RelationshipType.ManyToMany:
                                break;
                            default:
                                break;
                        }

                    }
                    else if (columns.TryGetValue(prop.ColumnName, out ordinal))
                    {
                        var val = dbReader[ordinal];
                        var fieldType = dbReader.GetFieldType(ordinal);
                        if (fieldType.Equals(keyVal.Value.PropertType))
                        {
                            keyVal.Value.Setter(instanceEntity, val);
                            logger.Log(LogType.Info, string.Format("Read {0}: {1}", keyVal.Key, val));
                        }
                        else if (keyVal.Value.PropertType.IsEnum)
                        {
                            var enumVal = TypeConverterStore.ConvertToEnum(keyVal.Value.PropertType, val);
                            keyVal.Value.Setter(instanceEntity, enumVal);
                            logger.Log(LogType.Info, string.Format("read {0}: value was {1}", keyVal.Key, enumVal));
                        }
                        else
                        {
                            var converter = TypeConverterStore.GetConverterFactoryMethod(keyVal.Value.PropertType);
                            keyVal.Value.Setter(instanceEntity, converter.Invoke(val));
                        }
                    }
                }
            }
        }

    }
}

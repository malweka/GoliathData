using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    using Diagnostics;
    using Mapping;
    using Providers;
    using Sql;

    //TODO make this internal class
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    class EntitySerializer : IEntitySerializer
    {

        static ConcurrentDictionary<Type, Delegate> factoryList = new ConcurrentDictionary<Type, Delegate>();

        static ILogger logger;
        internal ITypeConverterStore TypeConverterStore { get; set; }
        GetSetStore getSetStore = new GetSetStore();
        IDatabaseSettings settings;

        MapConfig Map
        {
            get { return settings.Map; }
        }

        /// <summary>
        /// Gets the SQL mapper.
        /// </summary>
        /// <value></value>
        public SqlMapper SqlMapper
        {
            get { return settings.SqlMapper; }
        }

        //DbAccess dbAccess;

        static EntitySerializer()
        {
            logger = Logger.GetLogger(typeof(EntitySerializer));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializer"/> class.
        /// </summary>
        /// <param name="sqlMapper">The SQL mapper.</param>
        /// <param name="map">The map.</param>
        public EntitySerializer(IDatabaseSettings settings) : this(settings, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializer"/> class.
        /// </summary>
        /// <param name="sqlMapper">The SQL mapper.</param>
        /// <param name="typeConverterStore">The type converter.</param>
        /// <param name="map">The map.</param>
        public EntitySerializer(IDatabaseSettings settings, ITypeConverterStore typeConverterStore)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            if (typeConverterStore == null)
                typeConverterStore = new TypeConverterStore();

            this.settings = settings;
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
        /// Sets the property value.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        public void SetPropertyValue(object entity, string propertyName, object propertyValue)
        {
            EntityGetSetInfo getSetInfo;
            Type typeOfInstance = entity.GetType();
            if (!getSetStore.TryGetValue(typeOfInstance, out getSetInfo))
            {
               //var map = Config.ConfigManager.CurrentSettings.Map;
                var entityMap = Map.GetEntityMap(typeOfInstance.FullName);

                getSetInfo = new EntityGetSetInfo(typeOfInstance);
                getSetInfo.Load(entityMap);
                getSetStore.Add(typeOfInstance, getSetInfo);
            }

            PropInfo pInfo;
            if (getSetInfo.Properties.TryGetValue(propertyName, out pInfo))
            {
                pInfo.Setter(entity, propertyValue);
            }
        }

        /// <summary>
        /// Creates the SQL worker.
        /// </summary>
        /// <returns></returns>
        public ISqlWorker CreateSqlWorker()
        {
            return new SqlWorker(SqlMapper, getSetStore, settings.ConverterStore);
        }

        /// <summary>
        /// Reads the field data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        public T ReadFieldData<T>(string fieldName, DbDataReader dataReader)
        {
            var val = ReadFieldData(typeof(T), fieldName, dataReader);
            return (T)val;
        }

        /// <summary>
        /// Reads the field data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        public T ReadFieldData<T>(int ordinal, DbDataReader dataReader)
        {
            var val = ReadFieldData(typeof(T), ordinal, dataReader);
            return (T)val;
        }

        /// <summary>
        /// Reads the field data.
        /// </summary>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        public object ReadFieldData(Type expectedType, string fieldName, DbDataReader dataReader)
        {
            int ordinal = dataReader.GetOrdinal(fieldName);
            return ReadFieldData(expectedType, ordinal, dataReader);
        }

        /// <summary>
        /// Reads the field data.
        /// </summary>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        public object ReadFieldData(Type expectedType, int ordinal, DbDataReader dataReader)
        {

            var dr = dataReader[ordinal];
            if ((dataReader[ordinal] != null) && (dataReader[ordinal] != DBNull.Value))
            {
                return ReadFieldData(expectedType, dr);
            }
            else
                return null;
        }

        public object ReadFieldData(Type expectedType, object value)
        {
            if (value == null)
                return null;

            Type actualType = value.GetType();
            if (actualType.Equals(expectedType))
                return value;
            else
            {
                var converter = TypeConverterStore.GetConverterFactoryMethod(expectedType);
                return converter.Invoke(value);
            }
        }

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
            EntityMap superEntityMap = null;
            if (entityMap.IsSubClass)
            {
                superEntityMap = entityMap.Parent.GetEntityMap(entityMap.Extends);
            }

            foreach (var keyVal in getSetInfo.Properties)
            {
                /* NOTE: Intentionally going only 1 level up the inheritance. something like :
                 *  SuperSuperClass
                 *      SuperClass
                 *          Class
                 *          
                 *  SuperSuperClass if is a mapped entity its properties will be ignored. May be implement this later on. 
                 *  For now too ugly don't want to touch.
                 */
                var prop = entityMap[keyVal.Key];
                if ((prop == null) && (superEntityMap != null))
                    prop = superEntityMap[keyVal.Key];
                int ordinal;
                if (prop != null)
                {
                    if (prop is Relation)
                    {
                        //logger.Log(LogType.Info, string.Format("Read {0} is a relation", keyVal.Key));
                        Relation rel = (Relation)prop;
                        switch (rel.RelationType)
                        {
                            case RelationshipType.ManyToOne:
                                SerializeManyToOne manyToOneHelper = new SerializeManyToOne(SqlMapper, getSetStore);
                                manyToOneHelper.Serialize(settings, this, rel, instanceEntity, keyVal.Value, entityMap, getSetInfo, columns, dbReader);
                                break;
                            case RelationshipType.OneToMany:
                                SerializeOneToMany oneToManyHelper = new SerializeOneToMany(SqlMapper, getSetStore);
                                oneToManyHelper.Serialize(settings, this, rel, instanceEntity, keyVal.Value, entityMap, getSetInfo, columns, dbReader);
                                break;
                            case RelationshipType.ManyToMany:
                                SerializeManyToMany manyToMany = new SerializeManyToMany(SqlMapper, getSetStore);
                                manyToMany.Serialize(settings, this, rel, instanceEntity, keyVal.Value, entityMap, getSetInfo, columns, dbReader);
                                break;
                            default:
                                break;
                        }

                    }
                    else if (columns.TryGetValue(prop.ColumnName, out ordinal))
                    {
                        var val = dbReader[ordinal];
                        var fieldType = dbReader.GetFieldType(ordinal);
                        if (fieldType.Equals(keyVal.Value.PropertType) && (val != DBNull.Value))
                        {
                            keyVal.Value.Setter(instanceEntity, val);
                        }
                        else if (keyVal.Value.PropertType.IsEnum)
                        {
                            var enumVal = TypeConverterStore.ConvertToEnum(keyVal.Value.PropertType, val);
                            keyVal.Value.Setter(instanceEntity, enumVal);
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

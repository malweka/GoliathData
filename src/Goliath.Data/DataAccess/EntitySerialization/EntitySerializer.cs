using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Goliath.Data.Collections;
using Goliath.Data.Diagnostics;
using Goliath.Data.DynamicProxy;
using Goliath.Data.Entity;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Utils;
using Goliath.Data.Sql;

namespace Goliath.Data.DataAccess
{
    [Serializable]
    internal class EntitySerializer : IEntitySerializer
    {
        private static readonly ConcurrentDictionary<Type, Delegate> factoryList =
            new ConcurrentDictionary<Type, Delegate>();

        private static ILogger logger;
        internal ITypeConverterStore TypeConverterStore { get; set; }
        private readonly EntityAccessorStore entityAccessorStore = new EntityAccessorStore();
        private readonly IDatabaseSettings settings;

        private MapConfig Map
        {
            get { return settings.Map; }
        }

        internal Func<ISession> SessionCreator { get; set; }


        public SqlDialect SqlDialect
        {
            get { return settings.SqlDialect; }
        }

        static EntitySerializer()
        {
            logger = Logger.GetLogger(typeof(EntitySerializer));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public EntitySerializer(IDatabaseSettings settings)
            : this(settings, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="typeConverterStore">The type converter.</param>
        public EntitySerializer(IDatabaseSettings settings, ITypeConverterStore typeConverterStore)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            if (typeConverterStore == null)
                typeConverterStore = new TypeConverterStore();

            this.settings = settings;
            TypeConverterStore = typeConverterStore;
        }

        public T CreateNewInstance<T>()
        {
            Type type = typeof(T);
            EntityMap entityMap = Map.GetEntityMap(type.FullName);
            return CreateNewInstance<T>(entityMap);
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityMap"></param>
        /// <returns></returns>
        public T CreateNewInstance<T>(EntityMap entityMap)
        {
            Type type = typeof(T);
            object instance = CreateNewInstance(type, entityMap);
            return (T)instance;
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">entityMap</exception>
        public object CreateNewInstance(Type type, EntityMap entityMap)
        {
            if (entityMap == null)
                throw new ArgumentNullException("entityMap");
            if (type == null)
                throw new ArgumentNullException("type");

            object instance;
            if (entityMap.IsTrackable)
                instance = type.CreateProxy(entityMap, true);
            else
                instance = Activator.CreateInstance(type);

            var getSetInfo = entityAccessorStore.GetEntityAccessor(type, entityMap);

            //load collections
            foreach (var rel in entityMap.Relations)
            {
                if ((rel.RelationType == RelationshipType.ManyToMany) ||
                    (rel.RelationType == RelationshipType.OneToMany))
                {
                    PropertyAccessor pInfo;
                    if (getSetInfo.Properties.TryGetValue(rel.PropertyName, out pInfo))
                    {
                        Type refEntityType = pInfo.PropertyType.GetGenericArguments().FirstOrDefault();
                        var collectionType = typeof(TrackableList<>).MakeGenericType(new Type[] { refEntityType });
                        var lazyCol = Activator.CreateInstance(collectionType);
                        pInfo.SetMethod(instance, lazyCol);
                    }
                }
            }

            return instance;
        }

        #region IEntitySerializerFactory Members

        /// <summary>
        /// Registers the data hydrator.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        public void RegisterDataHydrator<TEntity>(Func<DbDataReader, IEntityMap, TableQueryMap, IList<TEntity>> factoryMethod)
        {
            factoryList.TryAdd(typeof(TEntity), factoryMethod);
        }

        /// <summary>
        /// Serializes all.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="queryMap">The query map.</param>
        /// <returns></returns>
        /// <exception cref="GoliathDataException">unknown factory method</exception>
        public IList<TEntity> SerializeAll<TEntity>(DbDataReader dataReader, IEntityMap entityMap, TableQueryMap queryMap = null)
        {
            Delegate dlgMethod;
            Type type = typeof(TEntity);
            Func<DbDataReader, IEntityMap, TableQueryMap, IList<TEntity>> factoryMethod = null;

            if (factoryList.TryGetValue(type, out dlgMethod))
            {
                factoryMethod = dlgMethod as Func<DbDataReader, IEntityMap, TableQueryMap, IList<TEntity>>;

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

            IList<TEntity> entityList = factoryMethod(dataReader, entityMap, queryMap);
            return entityList;
        }


        /// <summary>
        /// Hydrates the specified instance to hydrate.
        /// </summary>
        /// <param name="instanceToHydrate">The instance to hydrate.</param>
        /// <param name="typeOfInstance">The type of instance.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="queryMap">The query map.</param>
        /// <param name="dataReader">The data reader.</param>
        public void Hydrate(object instanceToHydrate, Type typeOfInstance, EntityMap entityMap, TableQueryMap queryMap, DbDataReader dataReader)
        {
            if (dataReader.HasRows)
            {
                // Dictionary<string, int> columns = GetColumnNames(dataReader, entityMap.TableAlias);
                var entityAccessor = entityAccessorStore.GetEntityAccessor(typeOfInstance, entityMap);
                dataReader.Read();

                int count = 0;
                SerializeSingle(instanceToHydrate, typeOfInstance, entityMap, entityAccessor, queryMap, dataReader);
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
            Type typeOfInstance = entity.GetType();
            var entityMap = Map.GetEntityMap(typeOfInstance.FullName);
            var entityAccessor = entityAccessorStore.GetEntityAccessor(typeOfInstance, entityMap);
            PropertyAccessor pInfo;
            if (entityAccessor.Properties.TryGetValue(propertyName, out pInfo))
            {
                pInfo.SetMethod(entity, propertyValue);
            }
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
            if (object.Equals(actualType, expectedType)) //if (actualType.Equals(expectedType))
                return value;
            else
            {
                var converter = TypeConverterStore.GetConverterFactoryMethod(expectedType);
                return converter.Invoke(value);
            }
        }

        #endregion

        private Func<DbDataReader, IEntityMap, TableQueryMap, IList<TEntity>> CreateSerializerMethod<TEntity>(IEntityMap mapModel)
        {
            Func<DbDataReader, IEntityMap, TableQueryMap, IList<TEntity>> func = (dbReader, model, queryMap) =>
            {
                var list = new List<TEntity>();

                Type type = typeof(TEntity);

                EntityAccessor entityAccessor;
                //Dictionary<string, int> columns = null;

                if (model is EntityMap)
                {
                    var entityMap = (EntityMap)model;

                    if (queryMap == null)
                    {
                        int iteration = 0;
                        int recursion = 0;
                        queryMap = new TableQueryMap(entityMap.FullName, ref recursion, ref iteration);
                    }

                    LoadColumnIndexes(dbReader, queryMap);

                    //TODO: should we cache everything?
                    if (entityMap is DynamicEntityMap)
                    {
                        entityAccessor = new EntityAccessor(type);
                        entityAccessor.Load(entityMap);
                    }
                    else
                    {
                        entityAccessor = entityAccessorStore.GetEntityAccessor(type, entityMap);
                    }

                    while (dbReader.Read())
                    {
                        var instanceEntity = CreateNewInstance(type, entityMap);

                        int counter = 0;
                        SerializeSingle(instanceEntity, type, entityMap, entityAccessor, queryMap, dbReader);

                        list.Add((TEntity)instanceEntity);
                    }
                }
                else if (model is ComplexType)
                {
                    var complexType = (ComplexType)model;

                    if (queryMap == null)
                    {
                        //queryMap = new TableQueryMap(complexType.FullName);
                    }

                    throw new NotSupportedException("Serializing of complex types not yet supported.");

                    //LoadColumnIndexes(dbReader, queryMap);
                    // entityAccessor = entityAccessorStore.GetEntityAccessor(type, complexType);

                    //while (dbReader.Read())
                    //{
                    //    var instanceEntity = Activator.CreateInstance(type);
                    //    SerializeSingle(instanceEntity, type, complexType, entityAccessor, queryMap, dbReader);
                    //    list.Add((TEntity)instanceEntity);
                    //}
                }

                return list;
            };

            return func;
        }

        //This method below in case we want to implement eager loading.
        private bool CheckAndRemoveDupes(EntityMap entityMap, EntityAccessor accessor, object entity,
            Dictionary<object, object> resultStore)
        {
            if (entityMap.PrimaryKey != null)
            {
                var k = entityMap.PrimaryKey.Keys[0].Key;
                PropertyAccessor propAccessor = accessor.GetPropertyAccessor(k.PropertyName);
                var keyVal = propAccessor.GetMethod(entity);
                object storedEntity;
                if (resultStore.TryGetValue(keyVal, out storedEntity))
                {
                    //we have duplicate due to eager-loaded relations
                    foreach (var rel in entityMap.Relations)
                    {
                        if (!rel.LazyLoad)
                        {
                            var rPropAccessor = accessor.GetPropertyAccessor(rel.PropertyName);
                            var collection = rPropAccessor.GetMethod(storedEntity);
                            var newCollection = rPropAccessor.GetMethod(entity);

                            rPropAccessor.PropertyType.GetMethod("AddRange").Invoke(collection, new[] { newCollection });
                        }
                    }
                }
                else
                {
                    resultStore.Add(keyVal, entity);
                }
                return true;
            }
            return false;
        }

        //internal static Dictionary<string, int> GetColumnNames(DbDataReader dbReader, string tableAlias)
        //{
        //    var columns = new Dictionary<string, int>();

        //    for (var i = 0; i < dbReader.FieldCount; i++)
        //    {
        //        var fieldName = dbReader.GetName(i);

        //        if (!string.IsNullOrWhiteSpace(tableAlias))
        //        {
        //            var tabb = string.Format("{0}_", tableAlias);
        //            if (fieldName.StartsWith(tabb))
        //            {
        //                var colName = ParameterNameBuilderHelper.GetPropNameFromQueryName(fieldName, tableAlias);
        //                columns.Add(colName, i);
        //            }
        //        }
        //        else
        //        {
        //            columns.Add(fieldName, i);
        //        }
        //    }

        //    return columns;
        //}

        internal static void LoadColumnIndexes(DbDataReader dbReader, TableQueryMap queryMap)
        {
            for (var i = 0; i < dbReader.FieldCount; i++)
            {
                var fieldName = dbReader.GetName(i);
                TableQueryMap.ColumnInfo propName;

                if (queryMap.Columns.TryGetValue(fieldName, out propName))
                {
                    propName.Index = i;
                }
                else
                {
                    queryMap.Columns.Add(fieldName, new TableQueryMap.ColumnInfo { Index = i, PropertyName = fieldName });
                }
            }
        }

        internal void SerializeSingle(object instanceEntity, Type type, EntityMap entityMap,
            EntityAccessor entityAccessor, TableQueryMap queryMap, DbDataReader dbReader)
        {
            try
            {
                var trackable = instanceEntity as ITrackable;

                if (trackable != null)
                {
                    trackable.ChangeTracker.StopAndClear();
                    trackable.ChangeTracker.Init();
                }

                foreach (var columnInfo in queryMap.Columns)
                {
                    var prop = entityMap.GetProperty(columnInfo.Value.PropertyName);
                    if (prop == null)
                        continue;

                    ReadField(instanceEntity, trackable, entityAccessor, prop, type, columnInfo.Value.Index, dbReader);
                }

                int count = 0;

                foreach (var joinColumnQueryMap in queryMap.ReferenceColumns)
                {
                    var joinTable = settings.Map.GetEntityMap(joinColumnQueryMap.Value.JoinTable.Table);

                    if (entityMap.IsSubClass && count == 0)
                    {
                        //super class 
                        SerializeSingle(instanceEntity, type, joinTable, entityAccessor, queryMap, dbReader);
                    }
                    else
                    {
                        var rel = entityMap.GetProperty(joinColumnQueryMap.Value.ColumnName) as Relation;
                        if (rel == null) 
                            continue;

                        var accessor = entityAccessor.Properties[rel.PropertyName];
                        Type relType = accessor.PropertyType;

                        var relEntityAccessor = entityAccessorStore.GetEntityAccessor(relType, joinTable);
                        var relInstance = CreateNewInstance(relType, joinTable);

                        SerializeSingle(relInstance, relType, joinTable, relEntityAccessor, queryMap, dbReader);
                        accessor.SetMethod(instanceEntity, relInstance);
                    }

                    count++;
                }

                foreach (var rel in entityMap.Relations)
                {
                    var accessor = entityAccessor.Properties[rel.PropertyName];

                    switch (rel.RelationType)
                    {
                        case RelationshipType.ManyToOne:

                            if (!rel.LazyLoad)
                                continue;
                            var serializeManyToOne = new SerializeManyToOne(SqlDialect, entityAccessorStore);
                            serializeManyToOne.Serialize(settings, this, rel, instanceEntity, accessor, entityMap,
                                entityAccessor, dbReader);
                            break;
                        case RelationshipType.OneToMany:
                            var serializeOneToMany = new SerializeOneToMany(SqlDialect, entityAccessorStore);
                            serializeOneToMany.Serialize(settings, this, rel, instanceEntity, accessor, entityMap,
                                entityAccessor, dbReader);
                            break;
                        case RelationshipType.ManyToMany:
                            var serializeManyToMany = new SerializeManyToMany(SqlDialect, entityAccessorStore);
                            //serializeManyToMany.Serialize(settings, this, rel, instanceEntity, accessor, entityMap,
                            //    entityAccessor, dbReader);
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new GoliathDataException("Error trying to serialize " + entityMap.FullName + ".", exception);
            }
        }

        void ReadField(object instanceEntity, ITrackable trackable, EntityAccessor entityAccessor, Property prop, Type type, int ordinal, DbDataReader dbReader)
        {
            var accessor = entityAccessor.Properties[prop.PropertyName];
            if (accessor == null)
                throw new GoliathDataException("Could not find accessor for " + instanceEntity.ToString() + "." + prop.PropertyName);

            var val = dbReader[ordinal];
            var fieldType = dbReader.GetFieldType(ordinal);

            if ((fieldType == accessor.PropertyType) && (val != DBNull.Value))
            {
                accessor.SetMethod(instanceEntity, val);
                LoadInitialValueForInTracker(trackable, prop.PropertyName, val);
            }
            else if (accessor.PropertyType.IsEnum)
            {
                var enumVal = TypeConverterStore.ConvertToEnum(accessor.PropertyType, val);
                accessor.SetMethod(instanceEntity, enumVal);
                LoadInitialValueForInTracker(trackable, prop.PropertyName, enumVal);
            }
            else
            {
                var converter = TypeConverterStore.GetConverterFactoryMethod(accessor.PropertyType);
                var convertedValue = converter.Invoke(val);
                accessor.SetMethod(instanceEntity, convertedValue);
                LoadInitialValueForInTracker(trackable, prop.PropertyName, convertedValue);
            }
        }

        //internal bool SerializeSingle(object instanceEntity, Type type, EntityMap entityMap,
        //    EntityAccessor entityAccessor, Dictionary<string, int> columns, DbDataReader dbReader)
        //{
        //    var trackable = instanceEntity as ITrackable;
        //    bool pkHasValue = false;

        //    if (trackable != null)
        //    {
        //        //logger.Log(LogLevel.Debug, string.Format("Stopping and clearing Entity {0}  change tracker.", type));
        //        trackable.ChangeTracker.StopAndClear();
        //        //logger.Log(LogLevel.Debug, "Initializing trackable entity change tracker.");
        //        trackable.ChangeTracker.Init();
        //    }

        //    EntityMap superEntityMap = null;
        //    if (entityMap.IsSubClass)
        //    {
        //        superEntityMap = entityMap.Parent.GetEntityMap(entityMap.Extends);
        //    }

        //    foreach (var keyVal in entityAccessor.Properties)
        //    {
        //        /* NOTE: Intentionally going only 1 level up the inheritance. something like :
        //         *  SuperSuperClass
        //         *      SuperClass
        //         *          Class
        //         *          
        //         *  SuperSuperClass if is a mapped entity its properties will be ignored. May be implement this later on. 
        //         *  For now too ugly don't want to touch.
        //         */
        //        var prop = entityMap[keyVal.Key];

        //        if ((prop == null) && (superEntityMap != null))
        //            prop = superEntityMap[keyVal.Key];

        //        if (prop != null)
        //        {
        //            if (prop is Relation)
        //            {
        //                //logger.Log(LogLevel.Info, string.Format("Read {0} is a relation", keyVal.Key));
        //                var rel = (Relation)prop;
        //                switch (rel.RelationType)
        //                {
        //                    case RelationshipType.ManyToOne:
        //                        var manyToOneHelper = new SerializeManyToOne(SqlDialect, entityAccessorStore);
        //                        manyToOneHelper.Serialize(settings, this, rel, instanceEntity, keyVal.Value, entityMap,
        //                            entityAccessor, columns, dbReader);
        //                        break;
        //                    case RelationshipType.OneToMany:
        //                        var oneToManyHelper = new SerializeOneToMany(SqlDialect, entityAccessorStore);
        //                        oneToManyHelper.Serialize(settings, this, rel, instanceEntity, keyVal.Value, entityMap,
        //                            entityAccessor, columns, dbReader);
        //                        break;
        //                    case RelationshipType.ManyToMany:
        //                        var manyToMany = new SerializeManyToMany(SqlDialect, entityAccessorStore);
        //                        manyToMany.Serialize(settings, this, rel, instanceEntity, keyVal.Value, entityMap,
        //                            entityAccessor, columns, dbReader);
        //                        break;
        //                }
        //            }
        //            else
        //            {
        //                int ordinal;
        //                if (columns.TryGetValue(prop.ColumnName, out ordinal) ||
        //                    columns.TryGetValue(prop.ColumnName.ToLower(), out ordinal))
        //                {
        //                    var val = dbReader[ordinal];
        //                    var fieldType = dbReader.GetFieldType(ordinal);
        //                    if ((fieldType == keyVal.Value.PropertyType) && (val != DBNull.Value))
        //                    {
        //                        if (prop.IsPrimaryKey)
        //                            pkHasValue = true;
        //                        keyVal.Value.SetMethod(instanceEntity, val);
        //                        LoadInitialValueForInTracker(trackable, prop.PropertyName, val);
        //                    }
        //                    else if (keyVal.Value.PropertyType.IsEnum)
        //                    {
        //                        if (prop.IsPrimaryKey)
        //                            pkHasValue = true;

        //                        var enumVal = TypeConverterStore.ConvertToEnum(keyVal.Value.PropertyType, val);
        //                        keyVal.Value.SetMethod(instanceEntity, enumVal);
        //                        LoadInitialValueForInTracker(trackable, prop.PropertyName, enumVal);
        //                    }
        //                    else
        //                    {
        //                        var converter = TypeConverterStore.GetConverterFactoryMethod(keyVal.Value.PropertyType);
        //                        var convertedValue = converter.Invoke(val);
        //                        keyVal.Value.SetMethod(instanceEntity, convertedValue);
        //                        LoadInitialValueForInTracker(trackable, prop.PropertyName, convertedValue);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (trackable != null)
        //    {
        //        trackable.ChangeTracker.Start();
        //        trackable.Version = trackable.ChangeTracker.Version;
        //        //logger.Log(LogLevel.Debug, string.Format("Restarted tracker -- version {0}.", trackable.Version));
        //    }

        //    return pkHasValue;
        //}

        private void LoadInitialValueForInTracker(ITrackable trackable, string propertyName, object value)
        {
            if (trackable == null) return;
            trackable.ChangeTracker.LoadInitialValue(propertyName, value);
        }

        internal void SerializeSingle(object instanceEntity, Type type, ComplexType complextType,
            EntityAccessor entityAccessor, Dictionary<string, int> columns, DbDataReader dbReader)
        {
            var trackable = instanceEntity as ITrackable;
            if (trackable != null)
            {
                //logger.Log(LogLevel.Debug, string.Format("Stopping and clearing Entity {0}  change tracker.", type));
                trackable.ChangeTracker.StopAndClear();
                //logger.Log(LogLevel.Debug, "Initializing trackable entity change tracker.");
                trackable.ChangeTracker.Init();
            }

            foreach (var keyVal in entityAccessor.Properties)
            {
                /* NOTE: Intentionally going only 1 level up the inheritance. something like :
                 *  SuperSuperClass
                 *      SuperClass
                 *          Class
                 *          
                 *  SuperSuperClass if is a mapped entity its properties will be ignored. May be implement this later on. 
                 *  For now too ugly don't want to touch.
                 */
                var prop = complextType.GetProperty(keyVal.Key);
                if (prop != null)
                {
                    int ordinal;
                    if (columns.TryGetValue(prop.ColumnName, out ordinal))
                    {
                        var val = dbReader[ordinal];
                        var fieldType = dbReader.GetFieldType(ordinal);
                        if ((fieldType == keyVal.Value.PropertyType) && (val != DBNull.Value) && (val != null))
                        {
                            keyVal.Value.SetMethod(instanceEntity, val);
                            LoadInitialValueForInTracker(trackable, prop.PropertyName, val);
                        }
                        else if (keyVal.Value.PropertyType.IsEnum)
                        {
                            var enumVal = TypeConverterStore.ConvertToEnum(keyVal.Value.PropertyType, val);
                            keyVal.Value.SetMethod(instanceEntity, enumVal);
                            LoadInitialValueForInTracker(trackable, prop.PropertyName, enumVal);
                        }
                        else
                        {
                            var converter = TypeConverterStore.GetConverterFactoryMethod(keyVal.Value.PropertyType);
                            var convertedValue = converter.Invoke(val);
                            keyVal.Value.SetMethod(instanceEntity, convertedValue);
                            LoadInitialValueForInTracker(trackable, prop.PropertyName, convertedValue);
                        }
                    }
                }
            }

            if (trackable != null)
            {
                trackable.ChangeTracker.Start();
                trackable.Version = trackable.ChangeTracker.Version;
                //logger.Log(LogLevel.Debug, string.Format("Restarted tracker -- version {0}.", trackable.Version));
            }
        }
    }
}
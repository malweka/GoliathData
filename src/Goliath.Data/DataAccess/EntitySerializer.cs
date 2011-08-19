using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

using Goliath.Data.Mapping;
using System.Data.Common;
using Goliath.Data.Diagnostics;
using Goliath.Data.DynamicProxy;
using Goliath.Data.Sql;
using Goliath.Data.Providers;

namespace Goliath.Data.DataAccess
{
    //TODO make this internal class
    //TODO use singleton for entitySerializer;
    public class EntitySerializer : IEntitySerializer
    {

        static ConcurrentDictionary<Type, Delegate> factoryList = new ConcurrentDictionary<Type, Delegate>();

        static object lockFactoryList = new object();
        static ILogger logger;

        internal ITypeConverterStore TypeConverterStore { get; set; }

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
            //if (dbAccess == null)
            //    throw new ArgumentNullException("dbAccess");
            if (typeConverterStore == null)
                typeConverterStore = new TypeConverterStore();

            this.SqlMapper = sqlMapper;
            //this.dbAccess = dbAccess;
            this.TypeConverterStore = typeConverterStore;
        }

        #region IEntitySerializerFactory Members

        /// <summary>
        /// Registers the entity serializer.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        public void RegisterDataReaderEntitySerializer<TEntity>(Func<DbDataReader, EntityMap, TEntity> factoryMethod)
        {
            factoryList.TryAdd(typeof(TEntity), factoryMethod);
        }

        /// <summary>
        /// Serializes the specified data reader.
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
        public QueryInfo BuildInsertSql<TEntity>(EntityMap entityMap, TEntity entity)
        {
            InsertSqlBuilder sqlBuilder = new InsertSqlBuilder(SqlMapper, entityMap);

            Type type = typeof(TEntity);
            EntityGetSetInfo getSetInfo;

            if (!getSetStore.TryGetValue(type, out getSetInfo))
            {
                getSetInfo = new EntityGetSetInfo(type);
                getSetInfo.Load(entityMap);
                getSetStore.Add(type, getSetInfo);
            }

            if (entityMap.PrimaryKey != null)
            {
                foreach (var pk in entityMap.PrimaryKey.Keys)
                {
                    if (pk.KeyGenerator != null)
                    {
                        PropInfo pInfo;
                        if (getSetInfo.Properties.TryGetValue(pk.Key.PropertyName, out pInfo))
                        {
                            var id = pk.KeyGenerator.GenerateKey();
                            pInfo.Setter(entity, id);
                        }
                    }
                }
            }

            QueryInfo qInfo = new QueryInfo();
            qInfo.QuerySqlText = sqlBuilder.Build();
            qInfo.Parameters = BuildQuerParams(entity, getSetInfo, entityMap);

            return qInfo;
        }

        List<QueryParam> BuildQuerParams<TEntity>(TEntity entity, EntityGetSetInfo getSetInfo, EntityMap entityMap)
        {
            List<QueryParam> parameters = new List<QueryParam>();
            foreach (var prop in entityMap)
            {
                if (prop is Relation)
                {
                    Relation rel = (Relation)prop;
                    if (rel.RelationType != RelationshipType.ManyToOne)
                        continue;
                }

                PropInfo pInfo;
                if (getSetInfo.Properties.TryGetValue(prop.PropertyName, out pInfo))
                {
                    QueryParam param = new QueryParam(prop.ColumnName);
                    object val = pInfo.Getter(entity);

                    if ((val == null) && !prop.IsNullable)
                        throw new DataAccessException("{0}.{1} is cannot be null.", entityMap.Name, prop.PropertyName);
                    param.Value = val;
                    parameters.Add(param);
                }
                else
                    throw new MappingException(string.Format("Property {0} was not found for entity {1}", prop.PropertyName, entityMap.FullName));
            }

            return parameters;
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

        Dictionary<string, int> GetColumnNames(DbDataReader dbReader, string tableAbbreviation)
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
                        if (!rel.LazyLoad)
                        {
                            if (rel.RelationType == RelationshipType.ManyToOne)
                            {
                                var relEntMap = entityMap.Parent.EntityConfigs[rel.ReferenceEntityName];
                                if (relEntMap == null)
                                    throw new MappingException(string.Format("couldn't find referenced entity name {0} while try to build {1}", rel.ReferenceEntityName, entityMap.Name));

                                var relColumns = GetColumnNames(dbReader, relEntMap.TableAlias);
                                EntityGetSetInfo relGetSetInfo;
                                Type relType = keyVal.Value.PropertType;
                                if (!getSetStore.TryGetValue(relType, out relGetSetInfo))
                                {
                                    relGetSetInfo = new EntityGetSetInfo(relType);
                                    relGetSetInfo.Load(relEntMap);
                                    getSetStore.Add(relType, relGetSetInfo);
                                }

                                object relIstance = Activator.CreateInstance(relType);
                                SerializeSingle(relIstance, relType, relEntMap, relGetSetInfo, relColumns, dbReader);
                                keyVal.Value.Setter(instanceEntity, relIstance);
                                logger.Log(LogType.Info, string.Format("\t\t{0} is a ManyToOne", keyVal.Key));
                            }
                        }
                        else
                        {
                            if (rel.RelationType == RelationshipType.ManyToOne)
                            {
                                ProxyBuilder pbuilder = new ProxyBuilder();

                                var relEntMap = entityMap.Parent.EntityConfigs[rel.ReferenceEntityName];
                                if (relEntMap == null)
                                    throw new MappingException(string.Format("couldn't find referenced entity name {0} while try to build {1}", rel.ReferenceEntityName, entityMap.Name));

                                if (columns.TryGetValue(rel.ColumnName, out ordinal))
                                {
                                    var val = dbReader[ordinal];
                                    if (val != null)
                                    {
                                        QueryParam qp = new QueryParam(ParameterNameBuilderHelper.ColumnQueryName(relEntMap.TableAlias, rel.ReferenceColumn)) { Value = val };

                                        SelectSqlBuilder sqlBuilder = new SelectSqlBuilder(SqlMapper, relEntMap)
                                           .Where(new WhereStatement(ParameterNameBuilderHelper.ColumnWithTableAlias(relEntMap.TableAlias, rel.ReferenceColumn))
                                                    .Equals(SqlMapper.CreateParameterName(qp.Name)));

                                        QueryInfo qInfo = new QueryInfo();
                                        qInfo.QuerySqlText = sqlBuilder.Build();
                                        qInfo.Parameters = new QueryParam[] { qp };

                                        IProxyHydrator hydrator = new ProxySerializer(qInfo, keyVal.Value.PropertType, relEntMap, this);
                                        var proxyType = pbuilder.CreateProxy(keyVal.Value.PropertType, relEntMap);
                                        object proxyobj = Activator.CreateInstance(proxyType, new object[] { keyVal.Value.PropertType, hydrator });
                                        keyVal.Value.Setter(instanceEntity, proxyobj);
                                    }
                                }

                            }
                            else if (rel.RelationType == RelationshipType.OneToMany)
                            {
                                logger.Log(LogType.Info, string.Format("\t\t{0} is a OneToMany", keyVal.Key));
                                var propType = keyVal.Value.PropertType;
                                var relEntMap = entityMap.Parent.EntityConfigs[rel.ReferenceEntityName];
                                if (relEntMap == null)
                                    throw new MappingException(string.Format("couldn't find referenced entity name {0} while try to build {1}", rel.ReferenceEntityName, entityMap.Name));

                                Type refEntityType = propType.GetGenericArguments().FirstOrDefault();
                                if (refEntityType == null)
                                {
                                    throw new MappingException(string.Format("property type mismatch: {0} should be IList<T>", rel.PropertyName));
                                }

                                if (propType.Equals(typeof(IList<>).MakeGenericType(new Type[] { refEntityType })))
                                {
                                    if (columns.TryGetValue(rel.ColumnName, out ordinal))
                                    {
                                        var val = dbReader[ordinal];
                                        if (val != null)
                                        {
                                            QueryParam qp = new QueryParam(ParameterNameBuilderHelper.ColumnQueryName(relEntMap.TableAlias, rel.ReferenceColumn)) { Value = val };
                                            SelectSqlBuilder sqlBuilder = new SelectSqlBuilder(SqlMapper, relEntMap)
                                           .Where(new WhereStatement(ParameterNameBuilderHelper.ColumnWithTableAlias(relEntMap.TableAlias, rel.ReferenceColumn))
                                                    .Equals(SqlMapper.CreateParameterName(qp.Name)));

                                            QueryInfo qInfo = new QueryInfo();
                                            qInfo.QuerySqlText = sqlBuilder.Build();
                                            qInfo.Parameters = new QueryParam[] { qp };

                                            var collectionType = typeof(Collections.LazyList<>).MakeGenericType(new Type[] { refEntityType });
                                            var lazyCol = Activator.CreateInstance(collectionType, qInfo, relEntMap, this);
                                            keyVal.Value.Setter(instanceEntity, lazyCol);
                                        }
                                        else
                                        {
                                            var collectionType = typeof(List<>).MakeGenericType(new Type[] { refEntityType });
                                            keyVal.Value.Setter(instanceEntity, Activator.CreateInstance(collectionType));
                                        }
                                    }

                                }
                                else
                                    throw new MappingException(string.Format("property type mismatch: {0} should be IList<T>", rel.PropertyName));

                            }
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

        public static void DataReaderColumnList(IDataReader dataReader, EntityMap entMap)
        {
            List<string> ColumnNames = new List<string>();
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
            }
        }

    }
}

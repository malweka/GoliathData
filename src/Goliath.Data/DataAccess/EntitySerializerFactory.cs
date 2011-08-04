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
    //TODO use singleton for entitySerializerFactory;
    public class EntitySerializerFactory : IEntitySerializerFactory
    {

        static ConcurrentDictionary<Type, Delegate> factoryList = new ConcurrentDictionary<Type, Delegate>();

        static object lockFactoryList = new object();
        static ILogger logger;
        ITypeConverter typeConverter;
        SqlMapper sqlMapper;
        //DbAccess dbAccess;

        static EntitySerializerFactory()
        {
            logger = Logger.GetLogger(typeof(EntitySerializerFactory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializerFactory"/> class.
        /// </summary>
        public EntitySerializerFactory(SqlMapper sqlMapper) : this(sqlMapper, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializerFactory"/> class.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        public EntitySerializerFactory(SqlMapper sqlMapper,ITypeConverter typeConverter)
        {
            if (sqlMapper == null)
                throw new ArgumentNullException("sqlMapper");
            //if (dbAccess == null)
            //    throw new ArgumentNullException("dbAccess");
            if (typeConverter == null)
                typeConverter = new TypeConverter();

            this.sqlMapper = sqlMapper;
            //this.dbAccess = dbAccess;
            this.typeConverter = typeConverter;
        }

        #region IEntitySerializerFactory Members

        public void RegisterEntitySerializer<TEntity>(Func<DbDataReader, EntityMap, TEntity> factoryMethod)
        {
            throw new NotImplementedException();
        }

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

        public void Hydrate(object instanceToHydrate, Type typeOfInstance, EntityMap entityMap, DbDataReader dataReader)
        {
            if (dataReader.HasRows)
            {
                Dictionary<string, int> columns = GetColumnNames(dataReader, entityMap);
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

        GetSetStore getSetStore = new GetSetStore();

        #endregion

        Func<DbDataReader, EntityMap, IList<TEntity>> CreateSerializerMethod<TEntity>(EntityMap entityMap)
        {
            Func<DbDataReader, EntityMap, IList<TEntity>> func = (dbReader, entMap) =>
            {
                List<TEntity> list = new List<TEntity>();

                Type type = typeof(TEntity);
                Dictionary<string, int> columns = GetColumnNames(dbReader, entMap);

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

        Dictionary<string, int> GetColumnNames(DbDataReader dbReader, EntityMap entityMap)
        {
            Dictionary<string, int> columns = new Dictionary<string, int>();
            for (int i = 0; i < dbReader.FieldCount; i++)
            {
                var fieldName = dbReader.GetName(i);
                string tabb = string.Format("{0}_", entityMap.TableAbbreviation);
                if (fieldName.StartsWith(tabb))
                {
                    var colName = Property.GetPropNameFromQueryName(fieldName, entityMap);
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
                    if (columns.TryGetValue(keyVal.Key, out ordinal))
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
                            var enumVal = typeConverter.ConvertToEnum(keyVal.Value.PropertType, val);
                            keyVal.Value.Setter(instanceEntity, enumVal);
                            logger.Log(LogType.Info, string.Format("read {0}: value was {1}", keyVal.Key, enumVal));
                        }
                        else
                        {
                            var converter = typeConverter.GetConverter(keyVal.Value.PropertType);
                            keyVal.Value.Setter(instanceEntity, converter.Invoke(val));
                        }
                    }
                    else if (prop is Relation)
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

                                var relColumns = GetColumnNames(dbReader, relEntMap);
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
                        else //if (keyVal.Value.PropertType.IsGenericType)
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
                                    QueryParam qp = new QueryParam(string.Format("{0}_{1}", relEntMap.TableAbbreviation, rel.ReferenceColumn)) { Value = val };

                                    SelectSqlBuilder sqlBuilder = new SelectSqlBuilder(sqlMapper, relEntMap)
                                       .Where(new WhereStatement(string.Format("{0}.{1}", relEntMap.TableAbbreviation, rel.ReferenceColumn))
                                                .Equals(sqlMapper.CreateParameterName(qp.Name)));

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

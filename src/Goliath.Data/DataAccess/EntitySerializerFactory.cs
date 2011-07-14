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


namespace Goliath.Data.DataAccess
{
    //TODO make this internal class
    public class EntitySerializerFactory : IEntitySerializerFactory
    {

        static ConcurrentDictionary<Type, Delegate> factoryList = new ConcurrentDictionary<Type, Delegate>();
        static object lockFactoryList = new object();
        static ILogger logger;
        ITypeConverter typeConverter;

        static EntitySerializerFactory()
        {
            logger = Logger.GetLogger(typeof(EntitySerializerFactory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializerFactory"/> class.
        /// </summary>
        public EntitySerializerFactory() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializerFactory"/> class.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        public EntitySerializerFactory(ITypeConverter typeConverter)
        {
            if (typeConverter == null)
                typeConverter = new TypeConverter();

            this.typeConverter = typeConverter;
        }

        #region IEntitySerializerFactory Members

        public void RegisterEntitySerializer<TEntity>(Func<DbDataReader, EntityMap, TEntity> factoryMethod)
        {
            throw new NotImplementedException();
        }

        public IList<TEntity> Serialize<TEntity>(DbDataReader dataReader, EntityMap entityMap)
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

        GetSetStore getSetStore = new GetSetStore();
        #endregion

        Func<DbDataReader, EntityMap, IList<TEntity>> CreateSerializerMethod<TEntity>(EntityMap entityMap)
        {
            Func<DbDataReader, EntityMap, IList<TEntity>> func = (dbReader, entMap) =>
            {
                List<TEntity> list = new List<TEntity>();

                Type type = typeof(TEntity);
                Dictionary<string, int> colums = new Dictionary<string, int>();
                for (int i = 0; i < dbReader.FieldCount; i++)
                {
                    var fieldName = dbReader.GetName(i);
                    var colName = Property.GetPropNameFromQueryName(fieldName, entMap);
                    colums.Add(colName, i);
                }

                EntityGetSetInfo getSetInfo;
                if (!getSetStore.TryGetValue(type, out getSetInfo))
                {
                    getSetInfo = new EntityGetSetInfo(type);
                    getSetInfo.Load(entMap);
                }

                while (dbReader.Read())
                {
                    var instanceEntity = Activator.CreateInstance(type);

                    foreach (var keyVal in getSetInfo.Properties)
                    {
                        var prop = entityMap[keyVal.Key];
                        int ordinal;
                        if (prop != null)
                        {
                            if (colums.TryGetValue(keyVal.Key, out ordinal))
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
                            }
                        }
                    }

                    list.Add((TEntity)instanceEntity);
                }

                return list;
            };

            return func;
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

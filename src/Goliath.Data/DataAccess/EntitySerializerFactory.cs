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
    
    class EntitySerializerFactory : IEntitySerializerFactory
    {

        static ConcurrentDictionary<Type, Delegate> factoryList = new ConcurrentDictionary<Type, Delegate>();
        static object lockFactoryList = new object();
        static ILogger logger;

        static EntitySerializerFactory()
        {
            logger = Logger.GetLogger(typeof(EntitySerializerFactory));
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
                if (dlgMethod is Func<DbDataReader, EntityMap, TEntity>)
                    factoryMethod = (Func<DbDataReader, EntityMap, IList<TEntity>>)dlgMethod;
                else
                    throw new GoliathDataException("unknown factory method");
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
                        if ((prop != null) && colums.TryGetValue(keyVal.Key, out ordinal))
                        {
                            var val = dbReader[ordinal];
                            var fieldType = dbReader.GetFieldType(ordinal);
                            if (fieldType.Equals(keyVal.Value.PropertType))
                            {
                                keyVal.Value.Setter(instanceEntity, val);
                                logger.Log(LogType.Info, "voila");
                            }
                            else
                            {
                                logger.Log(LogType.Info, "need a type converter");
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

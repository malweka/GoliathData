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
    /// <summary>
    /// 
    /// </summary>
    public interface IEntitySerializerFactory
    {
        /// <summary>
        /// Registers the entity serializer.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        void RegisterEntitySerializer<TEntity>(Func<DbDataReader, EntityMap, TEntity> factoryMethod);
        /// <summary>
        /// Serializes the specified data reader.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        TEntity Serialize<TEntity>(DbDataReader dataReader, EntityMap entityMap);
    }

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

        public TEntity Serialize<TEntity>(DbDataReader dataReader, EntityMap entityMap)
        {
            Delegate dlgMethod;
            Type type = typeof(TEntity);
            Func<DbDataReader, EntityMap, TEntity> factoryMethod = null;
            if (factoryList.TryGetValue(type, out dlgMethod))
            {
                if (dlgMethod is Func<DbDataReader, EntityMap, TEntity>)
                    factoryMethod = (Func<DbDataReader, EntityMap, TEntity>)dlgMethod;
                else
                    throw new GoliathDataException("unknown factory method");
            }
            else
            {
                factoryMethod = CreateSerializerMethod<TEntity>(entityMap);
                factoryList.TryAdd(type, factoryMethod);
            }           

            TEntity entity = factoryMethod.Invoke(dataReader, entityMap);
            return entity;
        }

        GetSetStore getSetStore = new GetSetStore();
        #endregion

        //Dapper .Net inspired
        Func<DbDataReader, EntityMap, TEntity> CreateSerializerMethod<TEntity>(EntityMap entityMap)
        {
            Func<DbDataReader, EntityMap, TEntity> func = (dbReader, entMap) =>
            {
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
                    foreach (var keyVal in getSetInfo.Setters)
                    {
                        var prop = entityMap[keyVal.Key];
                        int ordinal;
                        if ((prop != null) && colums.TryGetValue(keyVal.Key, out ordinal))
                        {
                            var val = dbReader[ordinal];
                            logger.Log(LogType.Info, "voila");
                        }
                    }
                }

                return default(TEntity);
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

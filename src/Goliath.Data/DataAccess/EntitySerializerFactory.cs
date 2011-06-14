using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
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
        static Dictionary<Type, Delegate> factoryList = new Dictionary<Type, Delegate>();
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
            lock (lockFactoryList)
            {
                if (factoryList.TryGetValue(type, out dlgMethod))
                {
                    if (dlgMethod is Func<IDbAccess, IDataAccessAdapter<TEntity>>)
                        factoryMethod = (Func<DbDataReader, EntityMap, TEntity>)dlgMethod;
                    else
                        throw new DataAccessException("unknown factory method");
                }
                else
                {
                }
            }

            TEntity entity = factoryMethod.Invoke(dataReader, entityMap);
            return entity;
        }

        #endregion
    }
}

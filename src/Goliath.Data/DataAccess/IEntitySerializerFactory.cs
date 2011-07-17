using System;
using System.Collections.Generic;
using System.Data;
using Goliath.Data.Mapping;
using System.Data.Common;


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
        IList<TEntity> Serialize<TEntity>(DbDataReader dataReader, EntityMap entityMap);
    }
}

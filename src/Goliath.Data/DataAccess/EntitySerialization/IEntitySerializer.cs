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
    public interface IEntitySerializer
    {
        /// <summary>
        /// Gets the SQL mapper.
        /// </summary>
        Providers.SqlMapper SqlMapper { get; }

        /// <summary>
        /// Registers the data hydrator.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        void RegisterDataHydrator<TEntity>(Func<DbDataReader, EntityMap, TEntity> factoryMethod);

        /// <summary>
        /// Hydrates the specified instance to hydrate.
        /// </summary>
        /// <param name="instanceToHydrate">The instance to hydrate.</param>
        /// <param name="typeOfInstance">The type of instance.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="dataReader">The data reader.</param>
        void Hydrate(object instanceToHydrate, Type typeOfInstance, EntityMap entityMap, DbDataReader dataReader);


        /// <summary>
        /// Serializes all.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        IList<TEntity> SerializeAll<TEntity>(DbDataReader dataReader, EntityMap entityMap);

        /// <summary>
        /// Builds the insert SQL.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        QueryInfo BuildInsertSql<TEntity>(EntityMap entityMap, TEntity entity, bool recursive);

    }
}

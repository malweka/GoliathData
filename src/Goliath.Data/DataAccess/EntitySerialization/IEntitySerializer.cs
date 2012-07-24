using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    using Mapping;

    /// <summary>
    /// 
    /// </summary>
    public interface IEntitySerializer
    {
        /// <summary>
        /// Gets the SQL dialect.
        /// </summary>
        /// <value>The SQL dialect.</value>
        Providers.SqlDialect SqlDialect { get; }

        /// <summary>
        /// Hydrates the specified instance to hydrate.
        /// </summary>
        /// <param name="instanceToHydrate">The instance to hydrate.</param>
        /// <param name="typeOfInstance">The type of instance.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="dataReader">The data reader.</param>
        void Hydrate(object instanceToHydrate, Type typeOfInstance, EntityMap entityMap, DbDataReader dataReader);

        /// <summary>
        /// Registers the data hydrator.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        void RegisterDataHydrator<TEntity>(Func<DbDataReader, IEntityMap, TEntity> factoryMethod);

        /// <summary>
        /// Serializes all.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        IList<TEntity> SerializeAll<TEntity>(DbDataReader dataReader, IEntityMap entityMap);

        /// <summary>
        /// Creates the SQL worker.
        /// </summary>
        /// <returns></returns>
        Sql.ISqlWorker CreateSqlWorker();

        /// <summary>
        /// Reads the field data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        T ReadFieldData<T>(string fieldName, DbDataReader dataReader);

        /// <summary>
        /// Reads the field data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        T ReadFieldData<T>(int ordinal, DbDataReader dataReader);

        /// <summary>
        /// Reads the field data.
        /// </summary>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        object ReadFieldData(Type expectedType, string fieldName, DbDataReader dataReader);

        /// <summary>
        /// Reads the field data.
        /// </summary>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        object ReadFieldData(Type expectedType, int ordinal, DbDataReader dataReader);

        /// <summary>
        /// Reads the field data.
        /// </summary>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        object ReadFieldData(Type expectedType, object value);

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        void SetPropertyValue(object entity, string propertyName, object propertyValue);
    }
}

using System;
using System.Collections.Generic;
using Goliath.Data.Sql;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IDataAccessAdapter<TEntity> : IDataAccessAdapter, IDisposable
    {

        #region queries

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        IQueryBuilder<TEntity> Select();

        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <returns></returns>
        ICollection<TEntity> FetchAll();

        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        ICollection<TEntity> FetchAll(int limit, int offset);

        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="total">The total.</param>
        /// <returns></returns>
        ICollection<TEntity> FetchAll(int limit, int offset, out long total);

        ///// <summary>
        ///// Selects the specified column.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="propertyName">Name of the property.</param>
        ///// <param name="propertyNames">The property names.</param>
        ///// <returns></returns>
        //IQueryBuilder<T> Select<T>(string propertyName, params string[] propertyNames);

        #endregion

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        int Update(TEntity entity);

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        int Insert(TEntity entity, bool recursive = false);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cascade">if set to <c>true</c> [cascade].</param>
        /// <returns></returns>
        int Delete(TEntity entity, bool cascade = false);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IDataAccessAdapter { }
}

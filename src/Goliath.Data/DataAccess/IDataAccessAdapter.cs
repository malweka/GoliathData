using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    public interface IDataAccessAdapter { }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IDataAccessAdapter<TEntity>
    {

        #region Updates

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        int Update(TEntity entity);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        int Update(TEntity entity, QueryParam[] filters);
        /// <summary>
        /// Updates the batch.
        /// </summary>
        /// <param name="entityList">The entity list.</param>
        /// <returns></returns>
        int UpdateBatch(IEnumerable<TEntity> entityList);

        #endregion

        #region Inserts

        /// <summary>
        /// Inserts the batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <returns></returns>
        int InsertBatch(IEnumerable<TEntity> batch);

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        int Insert(TEntity entity);

        #endregion

        #region Queries

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        IList<TEntity> FindAll(string sqlQuery, params System.Data.Common.DbParameter[] parameters);

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        IList<TEntity> FindAll(params PropertyQueryParam[] filters);

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        IList<TEntity> FindAll(int limit, int offset, out long totalRecords, params PropertyQueryParam[] filters);
        
        /// <summary>
        /// Finds the one.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        TEntity FindOne(PropertyQueryParam filter, params PropertyQueryParam[] filters);

        #endregion

        #region Deletes

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        int Delete(TEntity entity);
        /// <summary>
        /// Deletes the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        int Delete(QueryParam[] filters);

        #endregion
    }
}

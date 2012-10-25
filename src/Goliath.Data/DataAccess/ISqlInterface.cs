using System.Collections.Generic;
using Goliath.Data.Sql;

namespace Goliath.Data 
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISqlInterface
    {
        #region queries

        /// <summary>
        /// Selects all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryBuilder<T> SelectAll<T>();

        /// <summary>
        /// Selects all.
        /// </summary>
        /// <returns></returns>
        ITableNameBuilder SelectAll();

        /// <summary>
        /// Selects the specified column.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyNames">The property names.</param>
        /// <returns></returns>
        IQueryBuilder<T> Select<T>(string propertyName, params string[] propertyNames);

        /// <summary>
        /// Selects the specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        ITableNameBuilder Select(string column, params string[] columns);

        #endregion
        
        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        int Update<T>(T entity);

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        int Insert<T>(T entity, bool recursive = false);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="cascade">if set to <c>true</c> [cascade].</param>
        /// <returns></returns>
        int Delete<T>(T entity, bool cascade = false);

        #region Run commands

        IList<T> RunList<T>(SqlQueryBody sql, int limit, int offset, params QueryParam[] paramArray);
        IList<T> RunList<T>(SqlQueryBody sql, int limit, int offset, out long total, params QueryParam[] paramArray);
        IList<T> RunList<T>(string sql, params QueryParam[] paramArray);
        IList<T> RunList<T>(SqlQueryBody sql, params QueryParam[] paramArray);

        T Run<T>(string sql, params QueryParam[] paramArray);
        T Run<T>(SqlQueryBody sql, params QueryParam[] paramArray);

        T RunMappedStatement<T>(string statementName, params QueryParam[] paramArray);
        T RunMappedStatement<T>(string statementName, QueryParam[] paramArray, params object[] inputParams);

        IList<T> RunListMappedStatement<T>(string statementName, params QueryParam[] paramArray);
        IList<T> RunListMappedStatement<T>(string statementName, QueryParam[] paramArray, params object[] inputParams);

        int RunNonQueryMappedStatement(string statementName, params QueryParam[] paramArray);
        //int RunNonQueryMappedStatement<T>(string statementName, params QueryParam[] paramArray);
        int RunNonQueryMappedStatement(string statementName, QueryParam[] paramArray, params object[] inputParams);

        #endregion

    }
}

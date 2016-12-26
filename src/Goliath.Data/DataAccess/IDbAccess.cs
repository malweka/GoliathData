using System.Collections.Generic;
using System.Data.Common;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDbAccess
    {
        #region Data access

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        int ExecuteNonQuery(DbConnection conn, string sql, params QueryParam[] parameters);

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        int ExecuteNonQuery(DbConnection conn, ITransaction transaction, string sql, params QueryParam[] parameters);

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        object ExecuteScalar(DbConnection conn, string sql, params QueryParam[] parameters);

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        object ExecuteScalar(DbConnection conn, ITransaction transaction, string sql, params QueryParam[] parameters);

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        DbDataReader ExecuteReader(DbConnection conn, string sql, params QueryParam[] parameters);

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        DbDataReader ExecuteReader(DbConnection conn, ITransaction transaction, string sql, params QueryParam[] parameters);

        /// <summary>
        /// Executes the dynamic.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        IList<dynamic> ExecuteDynamic(DbConnection conn, string sql, params QueryParam[] parameters);

        /// <summary>
        /// Executes the dictionary.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        IList<IDictionary<string, object>> ExecuteDictionary(DbConnection conn, string sql, params QueryParam[] parameters);

        #endregion
    }
}

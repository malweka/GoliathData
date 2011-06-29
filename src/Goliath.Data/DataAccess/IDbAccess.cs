using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDbAccess //: IDisposable
    {
        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="queryParam">The query param.</param>
        /// <returns></returns>
        DbParameter CreateParameter(QueryParam queryParam);

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        DbParameter CreateParameter(int i, object value);

        #region Data access

        ///// <summary>
        ///// Gets the current connection.
        ///// </summary>
        //DbConnection CurrentConnection { get; }



        ///// <summary>
        ///// Executes the non query.
        ///// </summary>
        ///// <param name="sql">The SQL.</param>
        ///// <param name="parameters">The parameters.</param>
        ///// <returns></returns>
        //int ExecuteNonQuery(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        int ExecuteNonQuery(DbConnection conn, string sql, params DbParameter[] parameters);

        ///// <summary>
        ///// Executes the scalar.
        ///// </summary>
        ///// <param name="sql">The SQL.</param>
        ///// <param name="parameters">The parameters.</param>
        ///// <returns></returns>
        //object ExecuteScalar(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        object ExecuteScalar(DbConnection conn, string sql, params DbParameter[] parameters);

        ///// <summary>
        ///// Executes the reader.
        ///// </summary>
        ///// <param name="sql">The SQL.</param>
        ///// <param name="parameters">The parameters.</param>
        ///// <returns></returns>
        //DbDataReader ExecuteReader(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        DbDataReader ExecuteReader(DbConnection conn, string sql, params DbParameter[] parameters);

        #endregion
    }
}

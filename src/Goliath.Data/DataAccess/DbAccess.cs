using System;
using System.Data.Common;
using Goliath.Data.Diagnostics;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class DbAccess : IDbAccess
    {
        #region properties and variables

        static ILogger logger;
        IDbConnector dbConnector;

        int? CommandTimeout
        {
            get
            {
                return dbConnector.CommandTimeout;
            }
        }

        #endregion

        #region .ctors

        static DbAccess()
        {
            logger = Logger.GetLogger(typeof(DbAccess));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbAccess"/> class.
        /// </summary>
        /// <param name="dbConnector">The db connector.</param>
        public DbAccess(IDbConnector dbConnector)
        {
            this.dbConnector = dbConnector;
        }

        #endregion

        #region Data access methods

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(DbConnection conn, string sql, params QueryParam[] parameters)
        {
            logger.Log(LogLevel.Debug, sql);
            return ExecuteNonQuery(conn, null, sql, parameters);
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(DbConnection conn, ITransaction transaction, string sql, params QueryParam[] parameters)
        {
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = conn;
                if (CommandTimeout != null)
                    cmd.CommandTimeout = CommandTimeout.Value;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var dbParam = dbConnector.CreateParameter(parameters[i].Name, parameters[i].Value);
                        cmd.Parameters.Add(dbParam);
                    }
                }

                if (transaction != null)
                    transaction.Enlist(cmd);

                logger.Log(LogLevel.Debug, sql);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public object ExecuteScalar(DbConnection conn, string sql, params QueryParam[] parameters)
        {
            return ExecuteScalar(conn, null, sql, parameters);
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public object ExecuteScalar(DbConnection conn, ITransaction transaction, string sql, params QueryParam[] parameters)
        {
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = conn;
                if (CommandTimeout != null)
                    cmd.CommandTimeout = CommandTimeout.Value;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var dbParam = dbConnector.CreateParameter(parameters[i].Name, parameters[i].Value);
                        cmd.Parameters.Add(dbParam);
                    }
                }

                if (transaction != null)
                    transaction.Enlist(cmd);

                logger.Log(LogLevel.Debug, sql);
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DbDataReader ExecuteReader(DbConnection conn, string sql, params QueryParam[] parameters)
        {
            logger.Log(LogLevel.Debug, sql);
            return ExecuteReader(conn, null, sql, parameters);
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DbDataReader ExecuteReader(DbConnection conn, ITransaction transaction, string sql, params QueryParam[] parameters)
        {
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = conn;
                if (CommandTimeout != null)
                    cmd.CommandTimeout = CommandTimeout.Value;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var dbParam = dbConnector.CreateParameter(parameters[i].Name, parameters[i].Value);
                        cmd.Parameters.Add(dbParam);
                    }
                }

                if (transaction != null)
                    transaction.Enlist(cmd);

                logger.Log(LogLevel.Debug, sql);
                return cmd.ExecuteReader();
            }
        }

        #endregion


    }
}

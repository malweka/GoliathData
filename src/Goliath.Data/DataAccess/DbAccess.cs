using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Goliath.Data.Mapping;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DbAccess : IDbAccess
    {
        #region properties and variables

        string connectionString;
        public string ConnectionString
        {
            get
            {
                //if (string.IsNullOrWhiteSpace(connectionString))
                //{
                //    if (!string.IsNullOrWhiteSpace(ProjectSettings.CurrentSettings.ConnectionString))
                //        connectionString = ProjectSettings.CurrentSettings.ConnectionString;
                //}
                return connectionString;
            }
            set { connectionString = value; }
        }

        public DbConnection Connection { get; protected set; }
        DbConnection transactedConnection;
        DbTransaction transaction;
        private int? commandTimeout;
        int transactionCount;

        public int? CommandTimeout
        {
            get { return commandTimeout; }
            set { commandTimeout = value; }
        }

        public string DatabaseProviderName { get; private set; }

        #endregion

        #region .ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="DbAccess"/> class.
        /// </summary>
        /// <param name="dbProviderName">Name of the db provider.</param>
        protected DbAccess(string dbProviderName)
        {
            DatabaseProviderName = dbProviderName;
        }

        #endregion

        #region Abstract methods

        public abstract DbConnection CreateNewConnection();

        public abstract DbParameter CreateParameter(int i, object value);

        public abstract DbParameter CreateParameter(string parameterName, object value);

        public virtual DbParameter CreateParameter(QueryParam queryParam)
        {
            if (queryParam == null)
                throw new ArgumentNullException("queryParam");

            return CreateParameter(queryParam.Name, queryParam.Value);
        }

        #endregion

        #region Database Connection methods

        #endregion

        #region Data access methods

        /// <summary>
        /// Disposes the transaction.
        /// </summary>
        private void disposeTransaction()
        {
            transaction = null;
            transactedConnection.Dispose();
            transactedConnection = null;
            //Commands.Clear();
        }


        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        public int ExecuteNonQuery(DbConnection conn, string sql, params DbParameter[] parameters)
        {
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = conn;
                if (CommandTimeout != null)
                    cmd.CommandTimeout = CommandTimeout.Value;
                if (transactedConnection != null)
                    cmd.Transaction = transaction;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        cmd.Parameters.Add(parameters[i]);
                    }
                }

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
        public object ExecuteScalar(DbConnection conn, string sql, params DbParameter[] parameters)
        {
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = conn;
                if (CommandTimeout != null)
                    cmd.CommandTimeout = CommandTimeout.Value;
                if (transactedConnection != null)
                    cmd.Transaction = transaction;
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        cmd.Parameters.Add(parameters[i]);
                    }
                }
                return cmd.ExecuteScalar();
            }
        }

        ///// <summary>
        ///// Executes the reader.
        ///// </summary>
        ///// <param name="sql">The SQL.</param>
        ///// <param name="parameters">The parameters.</param>
        ///// <returns></returns>
        //public DbDataReader ExecuteReader(string sql, params DbParameter[] parameters)
        //{
        //    using (DbConnection conn = CreateNewConnection())
        //    {
        //        conn.Open();
        //        return ExecuteReader(conn, sql, parameters);
        //    }
        //}

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DbDataReader ExecuteReader(DbConnection conn, string sql, params DbParameter[] parameters)
        {
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = conn;
                if (CommandTimeout != null)
                    cmd.CommandTimeout = CommandTimeout.Value;
                //if (transactedConnection != null)
                //    cmd.Transaction = transaction;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        cmd.Parameters.Add(parameters[i]);
                    }
                }

                return cmd.ExecuteReader();
            }
        }

        ///// <summary>
        ///// Executes the non query.
        ///// </summary>
        ///// <param name="sql">The SQL.</param>
        ///// <param name="parameters">The parameters.</param>
        //public int ExecuteNonQuery(string sql, params DbParameter[] parameters)
        //{
        //    using (DbConnection conn = CreateNewConnection())
        //    {
        //        return ExecuteNonQuery(conn, sql, parameters);
        //    }
        //}

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Connection != null)
                {
                    try
                    {
                        Connection.Dispose();
                    }
                    catch { }
                }

                if (transactedConnection != null)
                {
                    try
                    {
                        transactedConnection.Dispose();
                    }
                    catch { }
                }

                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }

        }

        #endregion

    }
}

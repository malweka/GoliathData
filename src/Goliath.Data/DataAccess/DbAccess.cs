﻿using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Goliath.Data
{
    using Diagnostics;
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
        /// <param name="dbProviderName">Name of the db provider.</param>
        public DbAccess(IDbConnector dbConnector)
        {
            this.dbConnector = dbConnector;
        }

        #endregion

        #region Connection and parameter methods

        public DbParameter CreateParameter(int i, object value)
        {
            return dbConnector.CreateParameter(i.ToString(), value);
        }

        public DbParameter CreateParameter(QueryParam queryParam)
        {
            if (queryParam == null)
                throw new ArgumentNullException("queryParam");
            try
            {
                object val = queryParam.Value;
                return dbConnector.CreateParameter(queryParam.Name, val);
            }
            catch (Exception ex)
            {
                throw new GoliathDataException(string.Format("Error while trying to create parameter {0}", queryParam.Name), ex);
            }
        }

        /// <summary>
        /// Creates the parameters.
        /// </summary>
        /// <param name="queryParams">The query params.</param>
        /// <returns></returns>
        public ICollection<DbParameter> CreateParameters(IEnumerable<QueryParam> queryParams)
        {
            if(queryParams == null)
                throw new ArgumentNullException("queryParams");
            List<DbParameter> parameters = new List<DbParameter>();
            foreach (var qp in queryParams)
                parameters.Add(CreateParameter(qp));

            return parameters;
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
        public int ExecuteNonQuery(DbConnection conn,  string sql, params DbParameter[] parameters)
        {
            return ExecuteNonQuery(conn, null, sql, parameters);
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        public int ExecuteNonQuery(DbConnection conn, ITransaction transaction, string sql, params DbParameter[] parameters)
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
                        cmd.Parameters.Add(parameters[i]);
                    }
                }

                if (transaction != null)
                    transaction.Enlist(cmd);

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
            return ExecuteScalar(conn, null, sql, parameters);
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public object ExecuteScalar(DbConnection conn, ITransaction transaction, string sql, params DbParameter[] parameters)
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
                        cmd.Parameters.Add(parameters[i]);
                    }
                }

                if (transaction != null)
                    transaction.Enlist(cmd);

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
        public DbDataReader ExecuteReader(DbConnection conn, string sql, params DbParameter[] parameters)
        {
            return ExecuteReader(conn, null, sql, parameters);
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DbDataReader ExecuteReader(DbConnection conn, ITransaction transaction, string sql, params DbParameter[] parameters)
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
                        cmd.Parameters.Add(parameters[i]);
                    }
                }

                if (transaction != null)
                    transaction.Enlist(cmd);

                return cmd.ExecuteReader();
            }
        }

        #endregion


    }
}

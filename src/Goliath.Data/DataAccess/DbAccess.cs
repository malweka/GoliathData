using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class DbAccess : IDbAccess
    {
        #region properties and variables

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

        internal DbConnection CreateConnection()
        {
            return dbConnector.CreateNewConnection();
        }

        public DbParameter CreateParameter(int i, object value)
        {
            return dbConnector.CreateParameter(i.ToString(), value);
        }

        public DbParameter CreateParameter(QueryParam queryParam)
        {
            if (queryParam == null)
                throw new ArgumentNullException("queryParam");

            return dbConnector.CreateParameter(queryParam.Name, queryParam.Value);
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
        public int ExecuteNonQuery(DbConnection conn, string sql, params DbParameter[] parameters)
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

        #endregion


    }
}

﻿using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Goliath.Data.Providers.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MssqlDbConnector : DbConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlDbConnector"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public MssqlDbConnector(string connectionString)
            : base(connectionString, RdbmsBackend.SupportedSystemNames.Mssql2008R2)
        {
        }

        //dispose of created connection? pool them if need be?
        public override bool AllowMultipleConnections
        {
            get { return true; }
        }

        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateNewConnection()
        {
            DbConnection retVal = new SqlConnection(ConnectionString);
            return retVal;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">parameterName</exception>
        public override DbParameter CreateParameter(string parameterName, object value, System.Data.DbType? dbType)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException("parameterName");

            if (value == null)
                value = DBNull.Value;

            var param = new SqlParameter(string.Format("@{0}", parameterName), value);
            return param;
        }
    }
}

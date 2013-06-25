using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
namespace Goliath.Data.Providers.Postgres
{
    /// <summary>
    /// 
    /// </summary>
    public class PostgresDbConnector : DbConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDbConnector"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public PostgresDbConnector(string connectionString)
            : base(connectionString, RdbmsBackend.SupportedSystemNames.Postgresql9)
        {
        }

        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
        public override System.Data.Common.DbConnection CreateNewConnection()
        {
            var connection = new NpgsqlConnection();
            return connection;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">parameterName</exception>
        public override System.Data.Common.DbParameter CreateParameter(string parameterName, object value)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException("parameterName");

            if (value == null)
                value = DBNull.Value;
            var param = new NpgsqlParameter(string.Format("${0}", parameterName), value);
            return param;
        }
    }
}

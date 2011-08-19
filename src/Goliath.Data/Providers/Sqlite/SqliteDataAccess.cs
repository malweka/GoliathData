using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.Providers.Sqlite
{
    public class SqliteDbConnector : DbConnector
    {
        static ILogger logger;

        static SqliteDbConnector()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDbConnector"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqliteDbConnector(string connectionString)
            : base(connectionString, Constants.ProviderName)
        {
            logger = Logger.GetLogger(typeof(SqliteDbConnector));
        }

        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
        public override System.Data.Common.DbConnection CreateNewConnection()
        {
            SQLiteConnection connection;
            connection = new SQLiteConnection(ConnectionString);
            return connection;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override System.Data.Common.DbParameter CreateParameter(string parameterName, object value)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException("parameterName");

            if (value == null)
                value = DBNull.Value;

            SQLiteParameter param;
            if (value is Guid)
            {
                logger.Log(LogType.Warning, "Convert guid to string for now. Please change before release.");
                param = new SQLiteParameter(string.Format("${0}", parameterName), value.ToString().ToUpper());
                logger.Log(LogType.Warning, string.Format("=== param name = {0}", param.ParameterName));
                logger.Log(LogType.Warning, string.Format("=== param value = {0}", param.Value));
                logger.Log(LogType.Warning, string.Format("=== param type = {0}", param.DbType));
            }
            else
                param = new SQLiteParameter(string.Format("${0}", parameterName), value);
            return param;
        }

    }
}

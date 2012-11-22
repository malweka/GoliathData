using System;
using System.Data.SQLite;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.Providers.Sqlite
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SqliteDbConnector : DbConnector
    {
        static ILogger logger;

        static SqliteDbConnector()
        {
            logger = Logger.GetLogger(typeof(SqliteDbConnector));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDbConnector"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqliteDbConnector(string connectionString)
            : base(connectionString, RdbmsBackend.SupportedSystemNames.Sqlite3)
        {
            AllowMultipleConnections = false;
        }

        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
        public override System.Data.Common.DbConnection CreateNewConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);
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

            var param = new SQLiteParameter(string.Format("${0}", parameterName), value);
            return param;
        }

    }
}

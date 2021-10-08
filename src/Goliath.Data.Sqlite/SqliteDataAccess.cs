using System;
using System.Data;
using System.Data.SQLite;
using Goliath.Data.Diagnostics;
using Goliath.Data.Providers;

namespace Goliath.Data.Sqlite
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SqliteDbConnector : DbConnector
    {
        ILogger logger = Logger.GetLogger(typeof(SqliteDbConnector));
        SQLiteConnection connection;

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple connections].
        /// </summary>
        public override bool AllowMultipleConnections => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDbConnector"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqliteDbConnector(string connectionString)
            : base(connectionString, RdbmsBackend.SupportedSystemNames.Sqlite3)
        {

        }

        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
        public override System.Data.Common.DbConnection CreateNewConnection()
        {
            if (connection == null)
                connection = new SQLiteConnection(ConnectionString);

            return connection;
        }

        public override IsolationLevel DefaultIsolationLevel => IsolationLevel.Unspecified;

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public override System.Data.Common.DbParameter CreateParameter(string parameterName, object value, DbType? dbType)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException(nameof(parameterName));

            if (value == null)
                value = DBNull.Value;

            var param = new SQLiteParameter($"${parameterName}", value);
            return param;
        }

    }
}

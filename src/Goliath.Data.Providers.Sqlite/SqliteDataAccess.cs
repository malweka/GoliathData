using System;
using System.Data;
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


        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple connections].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow multiple connections]; otherwise, <c>false</c>.
        /// </value>
        public override bool AllowMultipleConnections
        {
            get
            {
                return false;
            }
            protected set
            {

            }
        }

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

        public override IsolationLevel DefaultIsolationLevel
        {
            get { return IsolationLevel.Unspecified; }
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override System.Data.Common.DbParameter CreateParameter(string parameterName, object value, DbType? dbType)
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

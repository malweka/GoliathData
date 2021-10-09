using System;
using Goliath.Data.Providers;

namespace Goliath.Data.Sqlite
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SqliteProvider : IDbProvider
    {
        #region IDbProvider Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return RdbmsBackend.SupportedSystemNames.Sqlite3; }
        }

        /// <summary>
        /// Gets the database connector.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public IDbConnector GetDatabaseConnector(string connectionString)
        {
            return new SqliteDbConnector(connectionString);
        }

        /// <summary>
        /// Gets the SQL mapper.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        public SqlDialect SqlDialect
        {
            get { return new SqliteDialect(); }
        }

        #endregion
    }
}

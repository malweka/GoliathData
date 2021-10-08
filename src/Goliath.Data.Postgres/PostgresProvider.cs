using System;
using Goliath.Data.Providers;

namespace Goliath.Data.Postgres
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PostgresProvider : IDbProvider
    {
        #region IDbProvider Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return RdbmsBackend.SupportedSystemNames.Postgresql9; }
        }

        /// <summary>
        /// Gets the SQL dialect.
        /// </summary>
        /// <value>
        /// The SQL dialect.
        /// </value>
        public SqlDialect SqlDialect
        {
            get { return new PostgresDialect(); }
        }

        /// <summary>
        /// Gets the database connector.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public IDbConnector GetDatabaseConnector(string connectionString)
        {
            return new PostgresDbConnector(connectionString);
        }

        #endregion
    }
}

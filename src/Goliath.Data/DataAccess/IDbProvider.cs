﻿
namespace Goliath.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDbProvider
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the database connector.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        IDbConnector GetDatabaseConnector(string connectionString);


        /// <summary>
        /// Gets the SQL dialect.
        /// </summary>
        /// <value>The SQL dialect.</value>
        SqlDialect SqlDialect { get; }
    }
}

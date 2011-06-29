using System;
using System.Data;
using System.Data.Common;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDbConnector
    {
        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
        DbConnection CreateNewConnection();
        /// <summary>
        /// Gets the name of the database provider.
        /// </summary>
        /// <value>
        /// The name of the database provider.
        /// </value>
        string DatabaseProviderName { get; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        string ConnectionString { get; }

        /// <summary>
        /// Gets or sets the command timeout.
        /// </summary>
        /// <value>
        /// The command timeout.
        /// </value>
        int? CommandTimeout { get; set; }


        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        DbParameter CreateParameter(string parameterName, object value);


    }
}

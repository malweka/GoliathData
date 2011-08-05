using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace Goliath.Data.Providers
{
    public abstract class DbConnector : IDbConnector
    {
        /// <summary>
        /// Gets or sets the command timeout.
        /// </summary>
        /// <value>
        /// The command timeout.
        /// </value>
        public int? CommandTimeout { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// Gets the name of the database provider.
        /// </summary>
        /// <value>
        /// The name of the database provider.
        /// </value>
        public string DatabaseProviderName { get; protected set; }

        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
        public abstract DbConnection CreateNewConnection();

        ///// <summary>
        ///// Creates the parameter.
        ///// </summary>
        ///// <param name="i">The i.</param>
        ///// <param name="value">The value.</param>
        ///// <returns></returns>
        //public abstract DbParameter CreateParameter(int i, object value);

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public abstract DbParameter CreateParameter(string parameterName, object value);


        ///// <summary>
        ///// Creates the parameter.
        ///// </summary>
        ///// <param name="queryParam">The query param.</param>
        ///// <returns></returns>
        //public virtual DbParameter CreateParameter(QueryParam queryParam)
        //{
        //    if (queryParam == null)
        //        throw new ArgumentNullException("queryParam");

        //    return CreateParameter(queryParam.Name, queryParam.Value);
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnector"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="databaseProviderName">Name of the database provider.</param>
        protected DbConnector(string connectionString, string databaseProviderName)
        {
            ConnectionString = connectionString;
            DatabaseProviderName = databaseProviderName;
        }
    }
}

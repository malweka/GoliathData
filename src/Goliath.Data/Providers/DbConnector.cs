using System.Data;
using System.Data.Common;

namespace Goliath.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DbConnector : IDbConnector
    {
        #region Properties

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
        /// Gets or sets a value indicating whether [allow multiple connections].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [allow multiple connections]; otherwise, <c>false</c>.
        /// </value>
        public abstract bool AllowMultipleConnections { get;  }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
        public abstract DbConnection CreateNewConnection();

        /// <summary>
        /// Gets the default isolation level.
        /// </summary>
        /// <value>
        /// The default isolation level.
        /// </value>
        public virtual IsolationLevel DefaultIsolationLevel => IsolationLevel.ReadUncommitted;

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
        public abstract DbParameter CreateParameter(string parameterName, object value, DbType? dbType);

        #endregion

        #region .ctors

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

        #endregion

        #region Methods



        #endregion

    }
}

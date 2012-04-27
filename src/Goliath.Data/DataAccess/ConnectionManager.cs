using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionManager : IDisposable
    {
        IConnectionProvider connectionProvider;
        bool keepConnectionAlive;
        DbConnection currentConn;

        /// <summary>
        /// Gets the current connection.
        /// </summary>
        /// <value>The current connection.</value>
        public DbConnection CurrentConnection
        {
            get { return currentConn; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has open connection.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has open connection; otherwise, <c>false</c>.
        /// </value>
        public bool HasOpenConnection { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
        /// </summary>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="keepConnectionAlive">if set to <c>true</c> [keep connection alive].</param>
        public ConnectionManager(IConnectionProvider connectionProvider, bool keepConnectionAlive)
        {
            this.connectionProvider = connectionProvider;
            this.keepConnectionAlive = keepConnectionAlive;
            currentConn = connectionProvider.GetConnection();
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns></returns>
        public DbConnection OpenConnection()
        {
            if (!HasOpenConnection)
            {
                if (currentConn.State != ConnectionState.Open)
                    currentConn.Open();
                HasOpenConnection = true;
            }
            return currentConn;
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void CloseConnection()
        {
            if (!keepConnectionAlive)
            {
                connectionProvider.DiscardOfConnection(currentConn);
                currentConn = connectionProvider.GetConnection();
            }
            HasOpenConnection = false;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (currentConn != null)
            {
                connectionProvider.DiscardOfConnection(currentConn);

                if (keepConnectionAlive)
                {
                    DbConnection connRef = currentConn;
                    currentConn = null;
                }
            }
            
        }

        #endregion
    }

    //class ConnectionProvider
}

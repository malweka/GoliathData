using System;
using System.Data;
using System.Data.Common;


namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionManager : IDisposable, IConnectionManager
    {
        IDbConnector dbConnector;
        bool keepConnectionAlive;
        DbConnection currentConn;
        static ILogger logger;

        static ConnectionManager()
        {
            logger = Logger.GetLogger(typeof(ConnectionManager));
        }
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

        private bool userProvidedConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
        /// </summary>
        /// <param name="dbConnector">The db connector.</param>
        /// <param name="keepConnectionAlive">if set to <c>true</c> [keep connection alive].</param>
        public ConnectionManager(IDbConnector dbConnector, bool keepConnectionAlive)
            : this(dbConnector, null, keepConnectionAlive)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
        /// </summary>
        /// <param name="dbConnector">The db connector.</param>
        /// <param name="userConnection">The user connection.</param>
        /// <param name="keepConnectionAlive">if set to <c>true</c> [keep connection alive].</param>
        public ConnectionManager(IDbConnector dbConnector, DbConnection userConnection, bool keepConnectionAlive)
        {
            this.dbConnector = dbConnector;
            this.keepConnectionAlive = keepConnectionAlive;

            if (userConnection != null)
            {
                currentConn = userConnection;
                userProvidedConnection = true;
                this.keepConnectionAlive = true;

                if (userConnection.State == ConnectionState.Open)
                    HasOpenConnection = true;
            }
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns></returns>
        public DbConnection OpenConnection()
        {
            if (!HasOpenConnection)
            {
                if (!userProvidedConnection)
                    currentConn = dbConnector.CreateNewConnection();

                if (currentConn.State == ConnectionState.Open)
                {
                    logger.Log(LogLevel.Info, "Current connection is already open.");
                }
                else
                {
                    currentConn.Open();
                }
                
                HasOpenConnection = true;
            }
            else
            {
                if (currentConn.State == ConnectionState.Broken)
                {
                    try
                    {
                        currentConn.Close();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Closing broken connection failed.", ex);
                    }

                    currentConn = dbConnector.CreateNewConnection();
                    currentConn.Open();
                }
                else if (currentConn.State == ConnectionState.Closed)
                {
                    currentConn = dbConnector.CreateNewConnection();
                    currentConn.Open();
                }
            }

            return currentConn;
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void CloseConnection()
        {
            if (!keepConnectionAlive && HasOpenConnection)
            {
                currentConn.Close();
                currentConn = null;
                HasOpenConnection = false;
            }
            else if(userProvidedConnection)
            {
                currentConn = null;
                HasOpenConnection = false;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //if (currentConn != null)
            //{
            //    connectionProvider.DiscardOfConnection(currentConn);

            //    if (keepConnectionAlive)
            //    {
            //        DbConnection connRef = currentConn;
            //        currentConn = null;
            //    }
            //}

        }

        #endregion
    }

    //class ConnectionProvider
}

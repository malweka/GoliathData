using System;
using System.Data;
using System.Data.Common;


namespace Goliath.Data.DataAccess
{
    using Diagnostics;

    [Serializable]
    class ConnectionProvider : IConnectionProvider
    {
        static ILogger logger;
        IDbConnector connector;

        static ConnectionProvider()
        {
            logger = Logger.GetLogger(typeof(ConnectionProvider));
        }

        public ConnectionProvider(IDbConnector connector)
        {
            if (connector == null)
                throw new ArgumentNullException("connector");
            this.connector = connector;
        }

        #region IConnectionProvider Members

        public DbConnection GetConnection()
        {
            return connector.CreateNewConnection();
        }

        public DbConnection DiscardOfConnection(DbConnection dbConnection)
        {
            if (dbConnection != null)
            {
                try
                {
                    if (dbConnection.State == ConnectionState.Open)
                    {
                        dbConnection.Close();
                    }

                    dbConnection.Dispose();
                }
                catch (Exception ex)
                {
                    logger.LogException("DiscardOfConnection failed", ex);
                }
            }

            return dbConnection;
        }

        #endregion
    }

    [Serializable]
    class UserProvidedConnectionProvider : IConnectionProvider
    {
        DbConnection connection;

        public UserProvidedConnectionProvider(DbConnection connection)
        {
            this.connection = connection;
        }

        #region IConnectionProvider Members

        public DbConnection GetConnection()
        {
            return connection;
        }

        public DbConnection DiscardOfConnection(DbConnection dbConnection)
        {
            if (dbConnection == connection)
            {
                //de-reference the connection and send it back untouched
                DbConnection connectionRef = connection;
                connection = null;
                return connectionRef;
            }
            else
                throw new GoliathDataException("Couldn't discard this connection has it's not managed by this provider");
        }

        #endregion
    }
}

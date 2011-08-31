using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Goliath.Data.Providers;

namespace Goliath.Data.DataAccess
{
    [Serializable]
    class ConnectionProvider : IConnectionProvider
    {
        IDbConnector connector;

        public ConnectionProvider(IDbConnector connector)
        {
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
                    //log exception
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
                throw new GoliathDataException("couldn't discard this connection has it's not managed by this provider");
        }

        #endregion
    }
}

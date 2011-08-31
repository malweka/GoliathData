using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    public class ConnectionManager : IDisposable
    {
        IConnectionProvider connectionProvider;
        bool keepConnectionAlive;
        DbConnection currentConn;

        public DbConnection CurrentConnection
        {
            get { return currentConn; }
        }

        public bool HasOpenConnection { get; private set; }

        public ConnectionManager(IConnectionProvider connectionProvider, bool keepConnectionAlive)
        {
            this.connectionProvider = connectionProvider;
            this.keepConnectionAlive = keepConnectionAlive;
            currentConn = connectionProvider.GetConnection();
        }

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

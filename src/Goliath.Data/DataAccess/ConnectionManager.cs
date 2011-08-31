using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    public class ConnectionManager
    {
        IConnectionProvider connectionProvider;
        bool keepConnectionAlive;
        DbConnection currentConn;

        public ConnectionManager(IConnectionProvider connectionProvider, bool keepConnectionAlive)
        {
            this.connectionProvider = connectionProvider;
            this.keepConnectionAlive = keepConnectionAlive;

            if (keepConnectionAlive)
                currentConn = connectionProvider.GetConnection();
        }

        public IDbConnection OpenConnection()
        {
            if (keepConnectionAlive)
            {
                if (currentConn.State != ConnectionState.Open)
                    currentConn.Open();

                return currentConn;
            }
            else
            {
                currentConn = connectionProvider.GetConnection();
                currentConn.Open();
            }
            return currentConn;
        }

        public void CloseConnection()
        {
            connectionProvider.DiscardOfConnection(currentConn);
            DbConnection connRef = currentConn;
            currentConn = null;
        }
    }

    //class ConnectionProvider
}

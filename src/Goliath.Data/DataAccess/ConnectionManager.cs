using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    class ConnectionManager
    {
        IConnectionProvider connectionProvider;
        DbConnection currentConn;

        public ConnectionManager(IConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public IDbConnection OpenConnection()
        {
            throw new NotImplementedException();
        }

        public void CloseConnection()
        {
        }
    }

    //class ConnectionProvider
}

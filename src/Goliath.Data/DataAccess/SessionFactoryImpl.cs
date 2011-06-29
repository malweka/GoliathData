
namespace Goliath.Data.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DataAccess;
    using Diagnostics;

    class SessionFactoryImpl : ISessionFactory
    {
        IDbAccess dbAccess;
        IDbConnector dbConnector;

        public SessionFactoryImpl(IDbConnector dbConnector, IDbAccess dbAccess)
        {
            this.dbConnector = dbConnector;
            this.dbAccess = dbAccess;
        }

        #region ISessionFactory Members

        public ISession OpenSession(System.Data.IDbConnection connection)
        {
            var sess = new SessionImpl(dbAccess, connection);
            return sess;
        }

        public ISession OpenSession()
        {
            return OpenSession(dbConnector.CreateNewConnection());
        }

        #endregion
    }
}

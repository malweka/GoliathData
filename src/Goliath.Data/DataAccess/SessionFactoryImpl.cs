
namespace Goliath.Data.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DataAccess;
    using Diagnostics;

    [Serializable]
    class SessionFactoryImpl : ISessionFactory
    {
        IDbAccess dbAccess;
        IDbConnector dbConnector;
        IDataAccessAdapterFactory adapterFactory;

        public Config.ISessionSettings SessionSettings { get; private set; }

        public SessionFactoryImpl(IDbConnector dbConnector, IDbAccess dbAccess, IDataAccessAdapterFactory adapterFactory, Config.ISessionSettings settings)
        {
            this.dbConnector = dbConnector;
            this.dbAccess = dbAccess;
            this.adapterFactory = adapterFactory;
            SessionSettings = settings;
        }

        #region ISessionFactory Members

        public ISession OpenSession(System.Data.Common.DbConnection connection)
        {
            var sess = new SessionImpl(dbAccess, adapterFactory, connection);
            return sess;
        }

        public ISession OpenSession()
        {
            return OpenSession(dbConnector.CreateNewConnection());
        }

        #endregion
    }
}


namespace Goliath.Data.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DataAccess;
    using Diagnostics;
    using Mapping;

    [Serializable]
    class SessionFactoryImpl : ISessionFactory
    {
        //IDbAccess dbAccess;
        //IDbConnector dbConnector;
        Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> adapterFactoryFactoryMethod;
        IDataAccessAdapterFactory adapterFactory;
        IEntitySerializer serializer;

        public IDataAccessAdapterFactory AdapterFactory
        {
            get
            {
                if (adapterFactory == null)
                {
                    adapterFactory = adapterFactoryFactoryMethod.Invoke(DbSettings.Map, serializer);
                }
                return adapterFactory;
            }
        }

        public IDabaseSettings DbSettings { get; private set; }

        public SessionFactoryImpl(IDabaseSettings settings, Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> adapterFactoryFactoryMethod, IEntitySerializer serializer)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (adapterFactory == null)
                throw new ArgumentNullException("adapterFactory");

            adapterFactoryFactoryMethod = adapterFactoryFactoryMethod;
            DbSettings = settings;
        }

        ISession CreateSession(IConnectionProvider connProvider)
        {
            return new SessionImpl(this, connProvider);
        }

        #region ISessionFactory Members

        public ISession OpenSession(System.Data.Common.DbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            var sess = CreateSession(new UserProvidedConnectionProvider(connection));
            return sess;
        }

        public ISession OpenSession()
        {
            var sess = CreateSession(new ConnectionProvider(DbSettings.Connector));
            return sess;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

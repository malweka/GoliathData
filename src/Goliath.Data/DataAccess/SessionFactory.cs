using System;

namespace Goliath.Data.DataAccess
{
    using Mapping;

    [Serializable]
    class SessionFactory : ISessionFactory
    {
        //IDbAccess dbAccess;
        //IDbConnector dbConnector;
        Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> adapterFactoryFactoryMethod;
        IDataAccessAdapterFactory adapterFactory;
        IEntitySerializer serializer;


        public IEntitySerializer DataSerializer
        {
            get { return serializer; }
        }

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

        public IDatabaseSettings DbSettings { get; private set; }

        public SessionFactory(IDatabaseSettings settings, Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> adapterFactoryFactoryMethod, IEntitySerializer serializer)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (adapterFactoryFactoryMethod == null)
                throw new ArgumentNullException("adapterFactoryFactoryMethod");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            this.serializer = serializer;
            this.adapterFactoryFactoryMethod = adapterFactoryFactoryMethod;
            DbSettings = settings;
        }

        ISession CreateSession(IConnectionProvider connProvider)
        {
            return new Session(this, connProvider);
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

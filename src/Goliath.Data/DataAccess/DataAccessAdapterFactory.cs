using System;
using System.Collections.Concurrent;

using Goliath.Data.Mapping;

namespace Goliath.Data.DataAccess
{
    [Serializable]
    class DataAccessAdapterFactory : IDataAccessAdapterFactory
    {
        readonly ConcurrentDictionary<Type, Delegate> factoryList = new ConcurrentDictionary<Type, Delegate>();

        static ILogger logger;
        IEntitySerializer serializerFactory;
        bool isReady;
        MapConfig map;

        static DataAccessAdapterFactory()
        {
            logger = Logger.GetLogger(typeof(DataAccessAdapterFactory));           
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessAdapterFactory"/> class.
        /// </summary>
        public DataAccessAdapterFactory(MapConfig map, IEntitySerializer serializerFactory)
        {
            this.map = map;
            SetSerializerFactory(serializerFactory);
        }

        void SetSerializerFactory(IEntitySerializer serializerFactory)
        {
            if (serializerFactory == null)
                throw new ArgumentNullException("serializerFactory");

            this.serializerFactory = serializerFactory;
            isReady = true;
        }

        public void RegisterAdapter<TEntity>(Func<IEntitySerializer, ISession, IDataAccessAdapter<TEntity>> factoryMethod) where TEntity : class
        {
            var t = typeof(TEntity);
            factoryList.TryAdd(t, factoryMethod);
        }

        #region IDataAccessAdaterFactory Members

        public IDataAccessAdapter<TEntity> Create<TEntity>(IDbAccess dataAccess, ISession session)
        {
            try
            {
                Delegate dlgMethod;
                Type type = typeof(TEntity);
                Func<IEntitySerializer, ISession, IDataAccessAdapter<TEntity>> factoryMethod;

                if (factoryList.TryGetValue(type, out dlgMethod))
                {
                    if (dlgMethod is Func<IEntitySerializer, ISession, IDataAccessAdapter<TEntity>>)
                        factoryMethod = (Func<IEntitySerializer, ISession, IDataAccessAdapter<TEntity>>)dlgMethod;
                    else
                        throw new GoliathDataException("Unknown factory method");
                }
                else
                {
                    factoryMethod = CreateAdapter<TEntity>();
                    factoryList.TryAdd(type, factoryMethod);
                }

                if (!isReady)
                    throw new DataAccessException("DataAccessAdapter not ready. EntitySerializer was not set.", new InvalidOperationException("serializerFactory is null and not set."));

                IDataAccessAdapter<TEntity> adapter = factoryMethod(serializerFactory, session);
                return adapter;

            }
            catch (GoliathDataException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Error while trying to invoke DataAccessAdapter factory method for {0}", typeof(TEntity));
                throw new GoliathDataException(errorMessage, ex);
            }
        }

        #endregion

        internal Func<IEntitySerializer, ISession, IDataAccessAdapter<TEntity>> CreateAdapter<TEntity>()
        {
            Type type = typeof(TEntity);

            if (map != null)
            {
                EntityMap ent;
                if (map.EntityConfigs.TryGetValue(type.FullName, out ent))
                {
                    Func<IEntitySerializer, ISession, IDataAccessAdapter<TEntity>> myfunc = (sfactory, sess) => new DataAccessAdapter<TEntity>(sfactory, sess);
                    return myfunc;
                }

                throw new GoliathDataException(string.Format("{0} is not a mapped typed", type.FullName));
            }

            throw new GoliathDataException("Not entities map defined");
        }

        #region IDisposable Members

        public void Dispose()
        {
            MapConfig mapRef = this.map;
            map = null;

            IEntitySerializer serialRef = serializerFactory;
            serializerFactory = null;

            if (factoryList != null && factoryList.Count > 0)
            {
                factoryList.Clear();
            }
        }

        #endregion
    }
}

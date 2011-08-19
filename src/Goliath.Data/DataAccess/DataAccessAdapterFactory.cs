using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using System.Collections.Concurrent;

namespace Goliath.Data.DataAccess
{
    class DataAccessAdapterFactory : IDataAccessAdapterFactory
    {
        //IDbAccess db;
        //IDbConnector dbConnector;

        static ConcurrentDictionary<Type, Delegate> factoryList = new ConcurrentDictionary<Type, Delegate>();
        static object lockFactoryList = new object();
        static ILogger logger;
        IEntitySerializer serializerFactory;
        bool isReady;

        static DataAccessAdapterFactory()
        {
            logger = Logger.GetLogger(typeof(DataAccessAdapterFactory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessAdapterFactory"/> class.
        /// </summary>
        public DataAccessAdapterFactory()
        {
            //db = dataAccess;
            //this.dbConnector = dbConnector;
            
        }

        public void SetSerializerFactory(IEntitySerializer serializerFactory)
        {
            if (serializerFactory == null)
                throw new ArgumentNullException("serializerFactory");

            this.serializerFactory = serializerFactory;
            isReady = true;
        }

        public void RegisterAdapter<TEntity>(Func<IEntitySerializer, IDbAccess, DbConnection, IDataAccessAdapter<TEntity>> factoryMethod) where TEntity : class
        {
            var t = typeof(TEntity);
            factoryList.TryAdd(t, factoryMethod);
        }

        #region IDataAccessAdaterFactory Members

        public IDataAccessAdapter<TEntity> Create<TEntity>(IDbAccess dataAccess, DbConnection connection)
        {
            try
            {
                Delegate dlgMethod;
                IDataAccessAdapter<TEntity> adapter = null;
                Type type = typeof(TEntity);
                Func<IEntitySerializer, IDbAccess, DbConnection, IDataAccessAdapter<TEntity>> factoryMethod = null;

                if (factoryList.TryGetValue(type, out dlgMethod))
                {
                    if (dlgMethod is Func<IEntitySerializer, IDbAccess, DbConnection, IDataAccessAdapter<TEntity>>)
                        factoryMethod = (Func<IEntitySerializer, IDbAccess, DbConnection, IDataAccessAdapter<TEntity>>)dlgMethod;
                    else
                        throw new GoliathDataException("unknown factory method");
                }
                else
                {
                    factoryMethod = CreateAdapter<TEntity>();
                    factoryList.TryAdd(type, factoryMethod);
                }

                if (!isReady)
                    throw new DataAccessException("DataAccessAdapter not ready. EntitySerializer was not set.", new InvalidOperationException("serializerFactory is null and not set."));

                adapter = factoryMethod.Invoke(serializerFactory, dataAccess, connection);
                return adapter;

            }
            catch (GoliathDataException ex)
            {
                //logger.Log(string.Format("Error while trying to invoke DataAccessAdapter factory method for {0}", typeof(TEntity)), ex);
                throw;
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Error while trying to invoke DataAccessAdapter factory method for {0}", typeof(TEntity));
                logger.Log(errorMessage, ex);
                throw new GoliathDataException(errorMessage, ex); ;
            }
        }

        #endregion

        internal Func<IEntitySerializer, IDbAccess, DbConnection, IDataAccessAdapter<TEntity>> CreateAdapter<TEntity>()
        {
            Type type = typeof(TEntity);
            var map = Config.ConfigManager.CurrentSettings.Map;

            if (map != null)
            {
                EntityMap ent;
                if (map.EntityConfigs.TryGetValue(type.FullName, out ent))
                {
                    Func<IEntitySerializer, IDbAccess, DbConnection, IDataAccessAdapter<TEntity>> myfunc = (sfactory, dAccess, conn) => { return new DataAccessAdapter<TEntity>(sfactory, dAccess, conn); };
                    return myfunc;
                }

                throw new GoliathDataException(string.Format("{0} is not a mapped typed", type.FullName));
            }

            throw new GoliathDataException("Not entities map defined");
        }
    }
}

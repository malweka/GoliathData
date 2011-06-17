using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using System.Collections.Concurrent;
namespace Goliath.Data
{
    public interface IDataAccessAdaterFactory
    {
        IDataAccessAdapter<TEntity> Get<TEntity>();
        void RegisterAdapter<TEntity>(Func<IDbAccess, IDataAccessAdapter<TEntity>> factoryMethod) where TEntity : class;
    }

    class DataAccessAdapterFactory : IDataAccessAdaterFactory
    {
        IDbAccess db;
        static ConcurrentDictionary<Type, Delegate> factoryList = new ConcurrentDictionary<Type, Delegate>();
        static object lockFactoryList = new object();
        static ILogger logger;

        static DataAccessAdapterFactory()
        {
            logger = Logger.GetLogger(typeof(DataAccessAdapterFactory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessAdapterFactory"/> class.
        /// </summary>
        /// <param name="dataAccess">The data access.</param>
        public DataAccessAdapterFactory(IDbAccess dataAccess)
        {
            db = dataAccess;
        }

        public void RegisterAdapter<TEntity>(Func<IDbAccess, IDataAccessAdapter<TEntity>> factoryMethod) where TEntity : class
        {
            var t = typeof(TEntity);
            factoryList.TryAdd(t, factoryMethod);
        }

        #region IDataAccessAdaterFactory Members

        public IDataAccessAdapter<TEntity> Get<TEntity>()
        {
            try
            {
                Delegate dlgMethod;
                IDataAccessAdapter<TEntity> adapter = null;
                Type type = typeof(TEntity);
                Func<IDbAccess, IDataAccessAdapter<TEntity>> factoryMethod = null;

                if (factoryList.TryGetValue(type, out dlgMethod))
                {
                    if (dlgMethod is Func<IDbAccess, IDataAccessAdapter<TEntity>>)
                        factoryMethod = (Func<IDbAccess, IDataAccessAdapter<TEntity>>)dlgMethod;
                    else
                        throw new GoliathDataException("unknown factory method");
                }
                else
                {
                    factoryMethod = CreateAdapter<TEntity>();
                    factoryList.TryAdd(type, factoryMethod);
                }


                adapter = factoryMethod.Invoke(db);
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

        internal Func<IDbAccess, IDataAccessAdapter<TEntity>> CreateAdapter<TEntity>()
        {
            Type type = typeof(TEntity);
            var map = Config.ConfigManager.CurrentSettings.Map;

            if (map != null)
            {
                EntityMap ent;
                if (map.EntityConfigs.TryGetValue(type.FullName, out ent))
                {
                    Func<IDbAccess, IDataAccessAdapter<TEntity>> myfunc = x => { return new DataAccessAdapter<TEntity>(x); };
                    return myfunc;
                }

                throw new GoliathDataException(string.Format("{0} is not a mapped typed", type.FullName));
            }

            throw new GoliathDataException("Not entities map defined");
        }
    }
}

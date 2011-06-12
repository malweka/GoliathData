using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Goliath.Data.Diagnostics;

namespace Goliath.Data
{
    public interface IDataAccessAdaterFactory
    {
        IDataAccessAdapter<TEntity> Get<TEntity>() where TEntity : class;
        void RegisterAdapter<TEntity>(Func<IDbAccess, IDataAccessAdapter<TEntity>> factoryMethod) where TEntity : class;
    }

    class DataAccessAdapterFactory : IDataAccessAdaterFactory
    {
        IDbAccess db;
        Dictionary<Type, Delegate> factoryList = new Dictionary<Type, Delegate>();
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
            factoryList.Add(t, factoryMethod);
        }

        #region IDataAccessAdaterFactory Members

        public IDataAccessAdapter<TEntity> Get<TEntity>() where TEntity : class
        {
            try
            {
                Delegate dlgMethod;
                if (factoryList.TryGetValue(typeof(TEntity), out dlgMethod))
                {
                    var adapter = dlgMethod.DynamicInvoke(db);
                    if (adapter is IDataAccessAdapter<TEntity>)
                        return (IDataAccessAdapter<TEntity>)adapter;
                }
            }
            catch (Exception ex)
            {
                logger.Log(string.Format("Error while trying to invoke DataAccessAdapter factory method for {0}", typeof(TEntity)), ex);
            }

            return null;
        }

        #endregion

        IDataAccessAdapter<TEntity> CreateAdapter<TEntity>(IDbAccess dbAccess)
        {
            Type type = typeof(TEntity);
            var map = Config.ConfigManager.CurrentSettings.Map;

            if (map != null)
            {
                
            }

            throw new Exception();
        }
    }
}

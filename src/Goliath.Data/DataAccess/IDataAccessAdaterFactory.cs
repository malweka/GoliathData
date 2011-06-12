using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Goliath.Data
{
    public interface IDataAccessAdaterFactory
    {
        IDataAccessAdapter<TEntity> Get<TEntity>(System.Data.IDbConnection connection);
    }

    class DataAccessAdapterFactory : IDataAccessAdaterFactory
    {
        IDbAccess db;
        Dictionary<Type, Delegate> factoryList = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessAdapterFactory"/> class.
        /// </summary>
        /// <param name="dataAccess">The data access.</param>
        public DataAccessAdapterFactory(IDbAccess dataAccess)
        {
            db = dataAccess;
        }

        public void RegisterAdapter<TEntity>(Func<System.Data.IDbConnection, IDataAccessAdapter<TEntity>> factoryMethod)
        {
            var t = typeof(TEntity);
            factoryList.Add(t, factoryMethod);
        }

        #region IDataAccessAdaterFactory Members

        public IDataAccessAdapter<TEntity> Get<TEntity>(System.Data.IDbConnection connection)
        {
            try
            {
                Delegate dlgMethod;
                if (factoryList.TryGetValue(typeof(TEntity), out dlgMethod))
                {
                }
            }
            catch (Exception ex)
            {
            }
            throw new NotImplementedException();
        }

        #endregion
    }
}

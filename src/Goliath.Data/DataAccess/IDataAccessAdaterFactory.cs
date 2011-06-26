using System;
using System.Collections.Generic;


namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataAccessAdaterFactory
    {
        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        IDataAccessAdapter<TEntity> Get<TEntity>();
        /// <summary>
        /// Registers the adapter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        void RegisterAdapter<TEntity>(Func<IDbAccess, IDataAccessAdapter<TEntity>> factoryMethod) where TEntity : class;
    }

    
}

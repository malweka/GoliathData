using System;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataAccessAdapterFactory : IDisposable
    {
        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dataAccess">The data access.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        IDataAccessAdapter<TEntity> Create<TEntity>(IDbAccess dataAccess, ISession session);

        /// <summary>
        /// Registers the adapter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        void RegisterAdapter<TEntity>(Func<DataAccess.IEntitySerializer, ISession, IDataAccessAdapter<TEntity>> factoryMethod) where TEntity : class;
    }


}

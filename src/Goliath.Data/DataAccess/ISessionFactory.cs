using System;
using System.Data.Common;
using Goliath.Data.DataAccess;

namespace Goliath.Data
{  
    /// <summary>
    /// 
    /// </summary>
    public interface ISessionFactory : IDisposable
    {
        /// <summary>
        /// Gets the data serializer.
        /// </summary>
        IEntitySerializer DataSerializer { get; }

        /// <summary>
        /// Gets the session settings.
        /// </summary>
        IDatabaseSettings DbSettings { get; }

        /// <summary>
        /// Gets the adapter factory.
        /// </summary>
        /// <value>The adapter factory.</value>
        IDataAccessAdapterFactory AdapterFactory { get; }

        /// <summary>
        /// Opens the session.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        ISession OpenSession(DbConnection connection);

        /// <summary>
        /// Opens the session.
        /// </summary>
        /// <returns></returns>
        ISession OpenSession();

        /// <summary>
        /// Resets the connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        void ResetConnectionString(string connectionString);

    }
}

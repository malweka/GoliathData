using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Config
{
    /// <summary>
    /// Fluent interface for configuring our data access manager
    /// </summary>
    public interface IConfigurationManager
    {
        /// <summary>
        /// Loads the specified map file.
        /// </summary>
        /// <param name="mapFile">The map file.</param>
        /// <returns></returns>
        IConfigurationManager Load(string mapFile);
        /// <summary>
        /// Loads the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        IConfigurationManager Load(Mapping.MapConfig config);
        /// <summary>
        /// Providers the specified provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        IConfigurationManager Provider(Goliath.Data.Providers.IDbProvider provider);
        /// <summary>
        /// Registers the entity serializer.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <returns></returns>
        IConfigurationManager RegisterEntitySerializer<TEntity>(IEntitySerializer serializer);
        /// <summary>
        /// Loggers the specified logger.
        /// </summary>
        /// <param name="createLogger">The create logger.</param>
        /// <returns></returns>
        IConfigurationManager LoggerFactoryMethod(Func<Type,Diagnostics.ILogger> createLogger);

        /// <summary>
        /// Inits this instance.
        /// </summary>
        ISessionFactory Init();
    }

    public interface IConfigurationSettings
    {
        Mapping.MapConfig Map { get; }
        //Providers.IDbProvider DbProvider { get; }
    }
}

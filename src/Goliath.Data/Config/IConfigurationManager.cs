using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.DataAccess;

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
        /// Loggers the specified logger.
        /// </summary>
        /// <param name="createLogger">The create logger.</param>
        /// <returns></returns>
        IConfigurationManager LoggerFactoryMethod(Func<Type, Diagnostics.ILogger> createLogger);
        /// <summary>
        /// Providers the specified provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        IConfigurationManager Provider(Goliath.Data.Providers.IDbProvider provider);
        /// <summary>
        /// Overrides the data access adapter factory.
        /// </summary>
        /// <param name="dataAccessAdapter">The data access adapter.</param>
        /// <returns></returns>
        IConfigurationManager OverrideDataAccessAdapterFactory(IDataAccessAdapterFactory dataAccessAdapter);
        /// <summary>
        /// Overrides the type converter factory.
        /// </summary>
        /// <param name="typeConverterFactory">The type converter factory.</param>
        /// <returns></returns>
        IConfigurationManager OverrideTypeConverterStore(ITypeConverterStore typeConverterFactory);
        /// <summary>
        /// Overrides the entity serialize factory.
        /// </summary>
        /// <param name="entitySerializerFactory">The entity serializer factory.</param>
        /// <returns></returns>
        IConfigurationManager OverrideEntitySerializeFactory(IEntitySerializer entitySerializerFactory);
        /// <summary>
        /// Registers the type converter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="typeConverterFactoryMethod">The type converter factory method.</param>
        /// <returns></returns>
        IConfigurationManager RegisterTypeConverter<TEntity>(Func<Object, Object> typeConverterFactoryMethod);
        /// <summary>
        /// Inits this instance.
        /// </summary>
        ISessionFactory Init();
    }
    
}

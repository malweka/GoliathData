﻿using System;
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
        IConfigurationManager LoggerFactoryMethod(Func<Type, ILogger> createLogger);

        /// <summary>
        /// Registers the provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        IConfigurationManager RegisterProvider(Goliath.Data.Providers.IDbProvider provider);

        /// <summary>
        /// Overrides the data access adapter factory.
        /// </summary>
        /// <param name="dataAccessAdapter">The data access adapter.</param>
        /// <returns></returns>
        IConfigurationManager RegisterDataAccessAdapterFactory(Func<Mapping.MapConfig, IEntitySerializer, IDataAccessAdapterFactory> dataAccessAdapter);
        /// <summary>
        /// Overrides the type converter factory.
        /// </summary>
        /// <param name="converterStore">The type converter factory.</param>
        /// <returns></returns>
        IConfigurationManager RegisterTypeConverterStore(ITypeConverterStore converterStore);
        /// <summary>
        /// Overrides the entity serialize factory.
        /// </summary>
        /// <param name="entitySerializerFactory">The entity serializer factory.</param>
        /// <returns></returns>
        IConfigurationManager RegisterEntitySerializeFactory(IEntitySerializer entitySerializerFactory);
        /// <summary>
        /// Registers the type converter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="typeConverterFactoryMethod">The type converter factory method.</param>
        /// <returns></returns>
        IConfigurationManager AddTypeConverter<TEntity>(Func<Object, Object> typeConverterFactoryMethod);

        /// <summary>
        /// Inits this instance.
        /// </summary>
        ISessionFactory Init();
    }
    
}

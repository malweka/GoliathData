

namespace Goliath.Data.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Mapping;
    using Diagnostics;
    using Providers;
    using DataAccess;

    class ConfigManager : IConfigurationManager, IConfigurationSettings
    {
        MapConfig mainMap;
        IDbProvider provider;
        IDataAccessAdapterFactory dataAccessAdapterFactory;
        ITypeConverterStore typeConverterStore;
        IEntitySerializer entitySerializerFactory;
        internal Func<Type, ILogger> LoggerFactory { get; set; }

        public MapConfig Map
        {
            get
            {
                return mainMap;
            }
        }

        public IDbProvider DbProvider
        {
            get { return provider; }
            private set { provider = value; }
        }

        public ConfigManager(MapConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            mainMap = config;
            dataAccessAdapterFactory = new DataAccessAdapterFactory();
            typeConverterStore = new TypeConverterStore();
        }

        #region IConfigurationManager Members

        public IConfigurationManager Load(string mapFile)
        {
            MapConfig config = MapConfig.Create(mapFile);
            return Load(config);
        }

        public IConfigurationManager Load(MapConfig map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            mainMap.EntityConfigs.AddRange(map.EntityConfigs, mainMap);
            mainMap.ComplexTypes.AddRange(map.ComplexTypes, mainMap);
            return this;
        }

        public IConfigurationManager Provider(IDbProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            DbProvider = provider;
            return this;
        }

        /// <summary>
        /// Loggers the specified logger.
        /// </summary>
        /// <param name="createLogger">The create logger.</param>
        /// <returns></returns>
        public IConfigurationManager LoggerFactoryMethod(Func<Type, ILogger> createLogger)
        {
            LoggerFactory = createLogger;
            return this;
        }

        /// <summary>
        /// Overrides the data access adapter factory.
        /// </summary>
        /// <param name="dataAccessAdapter">The data access adapter.</param>
        /// <returns></returns>
        public IConfigurationManager OverrideDataAccessAdapterFactory(IDataAccessAdapterFactory dataAccessAdapter)
        {
            if (dataAccessAdapter == null)
                throw new ArgumentNullException("dataAccessAdapter");
            this.dataAccessAdapterFactory = dataAccessAdapter;
            return this;
        }

        public IConfigurationManager OverrideTypeConverterStore(ITypeConverterStore typeConverterStore)
        {
            if (typeConverterStore == null)
                throw new ArgumentNullException("typeConverterFactory");

            this.typeConverterStore = typeConverterStore;
            return this;
        }

        /// <summary>
        /// Overrides the entity serialize factory.
        /// </summary>
        /// <param name="entitySerializerFactory">The entity serializer factory.</param>
        /// <returns></returns>
        public IConfigurationManager OverrideEntitySerializeFactory(IEntitySerializer entitySerializerFactory)
        {
            if (entitySerializerFactory == null)
                throw new ArgumentNullException("entitySerializerFactory");

            this.entitySerializerFactory = entitySerializerFactory;
            return this;
        }

        /// <summary>
        /// Registers the type converter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="typeConverterFactoryMethod">The type converter factory method.</param>
        /// <returns></returns>
        public IConfigurationManager RegisterTypeConverter<TEntity>(Func<Object, Object> typeConverterFactoryMethod)
        {
            typeConverterStore.AddConverter(typeof(TEntity), typeConverterFactoryMethod);
            return this;
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public ISessionFactory Init()
        {
            if (DbProvider == null)
                throw new GoliathDataException("no database provider specified");

            if (LoggerFactory == null)
            {
                LoggerFactory = x => { return new ConsoleLogger(); };
            }
            if (entitySerializerFactory == null)
            {
                entitySerializerFactory = new EntitySerializer(DbProvider.SqlMapper, typeConverterStore);
            }

            dataAccessAdapterFactory.SetSerializerFactory(entitySerializerFactory);

            var dbConnector = DbProvider.GetDatabaseConnector(mainMap.Settings.ConnectionString);
            settings = this;

            var sessFact = new SessionFactoryImpl(dbConnector, new DbAccess(dbConnector), dataAccessAdapterFactory);
            return sessFact;
        }

        DbAccess IConfigurationSettings.CreateAccessor()
        {
            var dbConnector = DbProvider.GetDatabaseConnector(mainMap.Settings.ConnectionString);
            var dbAccess = new DbAccess(dbConnector);
            return dbAccess;
        }

        #endregion

        private static IConfigurationSettings settings;

        internal static IConfigurationSettings CurrentSettings
        {
            get { return settings; }
        }
    }
}

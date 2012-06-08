using System;

namespace Goliath.Data.Config
{

    using DataAccess;
    using Diagnostics;
    using Mapping;
    using Providers;

    class ConfigManager : IConfigurationManager, IDatabaseSettings
    {
        MapConfig mainMap;
        IDbProvider provider;
        IDbAccess dbAccess;
        IDbConnector connector;

        Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> dataAccessAdapterFactory;
        ITypeConverterStore typeConverterStore;
        IEntitySerializer entitySerializerFactory;
        internal Func<Type, ILogger> LoggerFactory { get; set; }

        public ITypeConverterStore ConverterStore
        {
            get { return typeConverterStore; }
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
            mainMap.UnprocessedStatements.AddRange(map.UnprocessedStatements);

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
        /// <param name="factoryMethod">The factory method.</param>
        /// <returns></returns>
        public IConfigurationManager OverrideDataAccessAdapterFactory(Func<MapConfig,IEntitySerializer,IDataAccessAdapterFactory> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException("factoryMethod");
            this.dataAccessAdapterFactory = factoryMethod;
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

            mainMap.MapStatements(DbProvider.Name);

            if (LoggerFactory == null)
            {
                LoggerFactory = x => { return new ConsoleLogger(); };
            }

            if (entitySerializerFactory == null)
            {
                entitySerializerFactory = new EntitySerializer(this, typeConverterStore);
            }

            if (dataAccessAdapterFactory == null)
            {
                dataAccessAdapterFactory = (map, serializer) => {
                    return new DataAccessAdapterFactory(map, serializer);
                };
            }

            var dbConnector = DbProvider.GetDatabaseConnector(mainMap.Settings.ConnectionString);
            settings = this;

            var sessFact = new SessionFactory(this, dataAccessAdapterFactory, entitySerializerFactory);
            return sessFact;
        }

       public  DbAccess CreateAccessor()
        {
            var dbConnector = DbProvider.GetDatabaseConnector(mainMap.Settings.ConnectionString);
            var dbAccess = new DbAccess(dbConnector);
            return dbAccess;
        }

        #endregion

        private static IDatabaseSettings settings;

        //internal static ISessionSettings CurrentSettings
        //{
        //    get { return settings; }
        //}

        #region ISessionSettings Members

        public MapConfig Map
        {
            get
            {
                return mainMap;
            }
        }

        public SqlMapper SqlMapper
        {
            get { return DbProvider.SqlMapper; }
        }

        public IDbConnector Connector
        {
            get
            {
                if (this.connector == null)
                    connector = this.DbProvider.GetDatabaseConnector(mainMap.Settings.ConnectionString);
                return connector;
            }
        }

        public IDbAccess DbAccess
        {
            get
            {
                if (dbAccess == null)
                    dbAccess = CreateAccessor();
                return dbAccess;
            }
        }

        #endregion
    }
}

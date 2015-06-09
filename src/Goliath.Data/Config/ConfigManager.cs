using System;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.Config
{
    class ConfigManager : IConfigurationManager, IDatabaseSettings
    {
        readonly MapConfig mainMap;
        IDbProvider provider;
        IDbAccess dbAccess;
        IDbConnector connector;

        Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> dataAccessAdapterFactory;
        ITypeConverterStore typeConverterStore;
        IEntitySerializer entitySerializerFactory;

        internal Func<Type, ILogger> LoggerFactory { get; set; }

        /// <summary>
        /// Gets the converter store.
        /// </summary>
        /// <value>
        /// The converter store.
        /// </value>
        public ITypeConverterStore ConverterStore
        {
            get { return typeConverterStore; }
        }

        /// <summary>
        /// Gets the db provider.
        /// </summary>
        /// <value>
        /// The db provider.
        /// </value>
        public IDbProvider DbProvider
        {
            get { return provider; }
            private set { provider = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigManager"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <exception cref="System.ArgumentNullException">config</exception>
        public ConfigManager(MapConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            mainMap = config;
            typeConverterStore = new TypeConverterStore();
        }

        #region IConfigurationManager Members

        /// <summary>
        /// Loads the specified map file.
        /// </summary>
        /// <param name="mapFile">The map file.</param>
        /// <returns></returns>
        public IConfigurationManager Load(string mapFile)
        {
            MapConfig config = MapConfig.Create(mapFile);
            return Load(config);
        }

        /// <summary>
        /// Loads the specified map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">map</exception>
        public IConfigurationManager Load(MapConfig map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            mainMap.EntityConfigs.AddRange(map.EntityConfigs, mainMap);
            mainMap.ComplexTypes.AddRange(map.ComplexTypes, mainMap);
            mainMap.UnprocessedStatements.AddRange(map.UnprocessedStatements);

            return this;
        }

        /// <summary>
        /// Registers the provider.
        /// </summary>
        /// <param name="dbProvider">The db provider.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">provider</exception>
        public IConfigurationManager RegisterProvider(IDbProvider dbProvider)
        {
            if (dbProvider == null)
                throw new ArgumentNullException("dbProvider");

            DbProvider = dbProvider;
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
        /// Registers the data access adapter factory.
        /// </summary>
        /// <param name="factoryMethod">The factory method.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">factoryMethod</exception>
        public IConfigurationManager RegisterDataAccessAdapterFactory(Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException("factoryMethod");

            dataAccessAdapterFactory = factoryMethod;
            return this;
        }

        /// <summary>
        /// Overrides the type converter factory.
        /// </summary>
        /// <param name="converterStore">The type converter factory.</param>
        /// <returns></returns>
        public IConfigurationManager RegisterTypeConverterStore(ITypeConverterStore converterStore)
        {
            if (converterStore == null)
                throw new ArgumentNullException("converterStore");

            typeConverterStore = converterStore;
            return this;
        }

        /// <summary>
        /// Overrides the entity serialize factory.
        /// </summary>
        /// <param name="entitySerializerFactory">The entity serializer factory.</param>
        /// <returns></returns>
        public IConfigurationManager RegisterEntitySerializeFactory(IEntitySerializer entitySerializerFactory)
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
        public IConfigurationManager AddTypeConverter<TEntity>(Func<Object, Object> typeConverterFactoryMethod)
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
                LoggerFactory = x => new ConsoleLogger();
            }

            if (entitySerializerFactory == null)
            {
                entitySerializerFactory = new EntitySerializer(this, typeConverterStore);
            }

            if (dataAccessAdapterFactory == null)
            {
                dataAccessAdapterFactory = (map, serializer) => new DataAccessAdapterFactory(map, serializer);
            }

            var dbConnector = DbProvider.GetDatabaseConnector(mainMap.Settings.ConnectionString);

            var sessFact = new SessionFactory(this, dataAccessAdapterFactory, entitySerializerFactory);
            return sessFact;
        }

        /// <summary>
        /// Creates the accessor.
        /// </summary>
        /// <returns></returns>
        public DbAccess CreateAccessor()
        {
            var dbConnector = DbProvider.GetDatabaseConnector(mainMap.Settings.ConnectionString);
            var accessor = new DbAccess(dbConnector);
            return accessor;
        }

        #endregion

        //internal static ISessionSettings CurrentSettings
        //{
        //    get { return settings; }
        //}

        #region ISessionSettings Members

        /// <summary>
        /// Gets the map.
        /// </summary>
        public MapConfig Map
        {
            get
            {
                return mainMap;
            }
        }

        /// <summary>
        /// Gets the SQL dialect.
        /// </summary>
        /// <value>
        /// The SQL dialect.
        /// </value>
        public SqlDialect SqlDialect
        {
            get { return DbProvider.SqlDialect; }
        }

        /// <summary>
        /// Gets the connector.
        /// </summary>
        /// <value>
        /// The connector.
        /// </value>
        public IDbConnector Connector
        {
            get
            {
                if (this.connector == null)
                    connector = this.DbProvider.GetDatabaseConnector(mainMap.Settings.ConnectionString);

                return connector;
            }
        }

        /// <summary>
        /// Gets the db access.
        /// </summary>
        /// <value>
        /// The db access.
        /// </value>
        public IDbAccess DbAccess
        {
            get { return dbAccess ?? (dbAccess = CreateAccessor()); }
        }

        #endregion

        public void ResetConnection(string connectionString)
        {
            if (!Map.Settings.SupportConnectionReset) return;

            Map.Settings.ConnectionString = connectionString;
            connector = null;
            dbAccess = null;

            var db = DbAccess;
            if (db == null)
                throw new GoliathDataException("couldn't reset connection ");
        }
    }
}

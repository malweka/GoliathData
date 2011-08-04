

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
            mainMap = config;
        }

        #region IConfigurationManager Members

        public IConfigurationManager Load(string mapFile)
        {
            MapConfig config = MapConfig.Create(mapFile);
            return Load(config);
        }

        public IConfigurationManager Load(MapConfig map)
        {
            mainMap.EntityConfigs.AddRange(map.EntityConfigs, mainMap);
            mainMap.ComplexTypes.AddRange(map.ComplexTypes, mainMap);
            return this;
        }

        public IConfigurationManager Provider(IDbProvider provider)
        {
            DbProvider = provider;
            return this;
        }

        public IConfigurationManager RegisterEntitySerializer<TEntity>(IEntitySerializer<TEntity> serializer)
        {
            return this;
        }

        public IConfigurationManager LoggerFactoryMethod(Func<Type, ILogger> createLogger)
        {
            LoggerFactory = createLogger;
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

            var dbConnector = DbProvider.GetDatabaseConnector(mainMap.Settings.ConnectionString);
            settings = this;

            var sessFact = new SessionFactoryImpl(dbConnector, new DbAccess(dbConnector));
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

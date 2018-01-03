using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data
{
    public class DatabaseProvider : IDatabaseProvider
    {
        //public string ConnectionStringName
        //{
        //    get
        //    {
        //        var connect = ConfigurationManager.AppSettings["connectionName"];
        //        if (string.IsNullOrWhiteSpace(connect))
        //            connect = "sqlServer";

        //        return connect;
        //    }
        //}

        public string ProviderName { get; private set; }

        public string ConnectionString { get; private set; }

        public IList<IKeyGenerator> KeyGenerators { get; private set; }
        public Dictionary<string, string> Providers { get; private set; }

        private readonly string configMapPath;

        public DatabaseProvider(string providerName, string configMapPath, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentNullException(nameof(providerName));

            if (string.IsNullOrWhiteSpace(configMapPath))
                throw new ArgumentNullException(nameof(configMapPath));

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ProviderName = providerName;
            ConnectionString = connectionString;

            this.configMapPath = configMapPath;
            KeyGenerators = new List<IKeyGenerator>();
            Providers = new Dictionary<string, string>()
            {
                {"SQLSERVER", "Goliath.Data.Providers.SqlServer.MssqlProvider, Goliath.Data" } ,
                {"POSTGRES", "Goliath.Data.Providers.Postgres.PostgresProvider, Goliath.Data.Providers.Postgres" } ,
                {"SQLITE", "Goliath.Data.Providers.Sqlite.SqliteProvider, Goliath.Data.Providers.Sqlite" } ,
            };
        }


        public ISessionFactory SessionFactory
        {
            get
            {
                if (!isInitialized)
                    Init();

                return sessionFactory;
            }
        }

        //private IDbProvider dbProvider;
        private static ISessionFactory sessionFactory;
        private bool isInitialized;

        public void Init()
        {
            if (isInitialized) return;
            var dbProvider = CreateDbProvider(ProviderName);

            var projSettings = new ProjectSettings
            {
                Platform = dbProvider.Name,
                Namespace = "Goliath.Data.Runtime", 
                ConnectionString = ConnectionString
            };

            sessionFactory = new Database()
                .Configure(configMapPath, projSettings, null, KeyGenerators.ToArray())
                .RegisterProvider(dbProvider).Init();

            isInitialized = true;
        }

        IDbProvider CreateDbProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName)) throw new Exception("Connection string provider name was not specified.");

            try
            {
                string providerTypeName;
                if(!Providers.TryGetValue(providerName.ToUpper(), out providerTypeName))
                    throw new Exception($"Provider [{providerName}] is not supported. Valid provider name: SqlServer, Sqlite, Postgres.");

                Type genServType = Type.GetType(providerTypeName);
                if (genServType == null) throw new Exception($"Could not create provider {providerTypeName}. Please make sure that the assembly is referenced.");
                var provider = (IDbProvider)Activator.CreateInstance(genServType);
                return provider;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        
    }
}
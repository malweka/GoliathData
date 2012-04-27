using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using System.IO;
using Goliath.Data.Providers;
using Goliath.Data.Providers.Sqlite;
using Goliath.Data.Mapping;
using Goliath.Data.Transformers;
using Goliath.Data.DataAccess;
using Goliath.Data.Sql;
using Goliath.Data;

namespace Goliath.Data.Tests
{
    [AssemblyFixture]
    public class AssemblySetupTestFixture
    {
        const string MapFileName = "GoData.Map.xml";

        [FixtureSetUp]
        public void SetUp()
        {
            string pdir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("src"));

            string autoIncrementFileName = "ZooDb_auto_increment.db";
            //string guidFileName = string.Format("ZooDb_guid.{0}.db",  DateTime.Now.Ticks);

            string mappingFolder = Path.Combine(pdir, "src", "Goliath.Data.Tests", "Entities", "AutoIncrement");
            string scriptFolder = Path.Combine(pdir, "src", "Goliath.Data.Tests", "Scripts", "Sqlite", "AutoIncrement");
            string dbfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, autoIncrementFileName);
            string cs = string.Format("Data Source={0}; Version=3", dbfile);

            
            string mapfile = Path.Combine(mappingFolder, MapFileName);

            MapConfig config = MapConfig.Create(mapfile);
            config.Settings.ConnectionString = cs;
            DatabaseInit.CreateSqliteDatabase(config, scriptFolder, dbfile);

            var sessionFactory = new Database().Configure(config)
                .Provider(new SqliteProvider()).Init();

            SessionHelper.SetFactory(sessionFactory);
        }
    }

    public class SessionHelper
    {
        public static ISessionFactory Factory { get; private set; }

        public static string BaseDirectory
        {
            get
            {
                string pdir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("src"));
                string basefolder = Path.Combine(pdir, "src", "Goliath.Data.Tests");
                return basefolder;
            }
        }


        public static void SetFactory(ISessionFactory factory)
        {
            if (Factory == null)
            {
                Factory = factory;
            }
        }
    }
}

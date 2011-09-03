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
            string mappingFolder = Path.Combine(pdir, "src", "Goliath.Data.Console", "Generated", "Sqlite");
            string dbfile = Path.Combine(pdir, "src", "Goliath.Data.Console", "Data", "WebZoo.db");
            string cs = string.Format("Data Source={0}; Version=3", dbfile);

            string mapfile = Path.Combine(mappingFolder, MapFileName);

            var sessionFactory = new Database().Configure(mapfile, cs)
                .Provider(new SqliteProvider()).Init();

            SessionHelper.SetFactory(sessionFactory);
        }
    }

    public class SessionHelper
    {
        public static ISessionFactory Factory { get; private set; }

        public static void SetFactory(ISessionFactory factory)
        {
            if (Factory == null)
            {
                Factory = factory;
            }
        }
    }
}

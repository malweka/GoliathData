using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Goliath.Data.Providers;
using Goliath.Data.Providers.Sqlite;
using Goliath.Data.Providers.SqlServer;
using Goliath.Data.Generators;
using System.IO;
using Goliath.Data.Mapping;
using Goliath.Data.Transformers;
namespace Goliath.Data.Tests
{
    [AssemblyFixture]
    public class AssemblyTestFixture
    {
        const string MapFileName = "GoData.Map.xml";

        static AssemblyTestFixture()
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.LastIndexOf("Goliath.Data.Tests"));
            string mapfile = Path.Combine(currentDir, "Goliath.Data.Console", "Generated", "Sqlite", MapFileName);
            var sessionFactory = new Database().Configure(mapfile)
                .Provider(new SqliteProvider()).Init();

            var sess = sessionFactory.OpenSession();
        }
    }
}

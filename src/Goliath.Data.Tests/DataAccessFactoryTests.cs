using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using WebZoo.Data.Sqlite;
using Goliath.Data.DataAccess;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class DataAccessFactoryTests
    {
        [Test]
        public void Register_Adapter_And_Make_Sure_You_Can_get_back()
        {
            var dbAccess = new Providers.Sqlite.SqliteDataAccess(@"Data Source=C:\Junk\Goliath.Data\WebZoo.db; Version=3");
            IDataAccessAdaterFactory dfactory = new DataAccessAdapterFactory(dbAccess);
            dfactory.RegisterAdapter<Monkey>(x => { return new DataAccessAdapter<Monkey>(x); });

            var adapter = dfactory.Get<Monkey>();
            Assert.IsInstanceOfType<DataAccessAdapter<Monkey>>(adapter);
        }

        [Test]
        public void Create_adapter_should_not_throw()
        {
            var dbAccess = new Providers.Sqlite.SqliteDataAccess(@"Data Source=C:\Junk\Goliath.Data\WebZoo.db; Version=3");
            DataAccessAdapterFactory dfactory = new DataAccessAdapterFactory(dbAccess);
            var adapterFactory = dfactory.CreateAdapter<Monkey>();
            var adapter = adapterFactory.Invoke(dbAccess);
            Assert.IsInstanceOfType<DataAccessAdapter<Monkey>>(adapter);
        }
    }
}

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
            var dbConnector = new Providers.Sqlite.SqliteDbConnector(@"Data Source=C:\Junk\Goliath.Data\WebZoo.db; Version=3");
            var dbAccess = new DbAccess(dbConnector);

            IDataAccessAdaterFactory dfactory = new DataAccessAdapterFactory(dbAccess, dbConnector);
            dfactory.RegisterAdapter<Monkey>((x, y) => { return new DataAccessAdapter<Monkey>(y,x); });

            var adapter = dfactory.Get<Monkey>();
            Assert.IsInstanceOfType<DataAccessAdapter<Monkey>>(adapter);
        }

        [Test]
        public void Create_adapter_should_not_throw()
        {
            var dbConnector = new Providers.Sqlite.SqliteDbConnector(@"Data Source=C:\Junk\Goliath.Data\WebZoo.db; Version=3");
            var dbAccess = new DbAccess(dbConnector); 
            DataAccessAdapterFactory dfactory = new DataAccessAdapterFactory(dbAccess, dbConnector);
            var adapterFactory = dfactory.CreateAdapter<Monkey>();
            var adapter = adapterFactory.Invoke(dbAccess, dbConnector);
            Assert.IsInstanceOfType<DataAccessAdapter<Monkey>>(adapter);
        }
    }
}

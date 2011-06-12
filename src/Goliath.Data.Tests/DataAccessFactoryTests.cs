using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class DataAccessFactoryTests
    {
        [Test]
        public void Register_Adapter_And_Make_Sure_You_Can_get_back()
        {
            var dbAccess = new Providers.SqlServer.MssqlDataAccess("Data Source=localhost;Initial Catalog=DbZoo;Integrated Security=True");
            IDataAccessAdaterFactory dfactory = new DataAccessAdapterFactory(dbAccess);
            dfactory.RegisterAdapter<Monkey>(x => { return new DataAccessAdapter<Monkey>(x); });

            var adapter = dfactory.Get<Monkey>();
            Assert.IsInstanceOfType<DataAccessAdapter<Monkey>>(adapter);
        }
    }
}

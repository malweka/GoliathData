using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.DataAccess;
using Goliath.Data.Sql;
using NUnit.Framework;
using WebZoo.Data;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class QueryBuilderGenericTests
    {
        ISession session;

        [TestFixtureSetUp]
        public void Initialize()
        {
            session = SessionHelper.Factory.OpenSession();
        }

        [Test]
        public void Retrieve_right_entity_based_on_type()
        {
            var query = session.SelectAll<Animal>() as QueryBuilder<Animal>;
            Assert.IsNotNull(query);
            Assert.AreEqual(typeof(Animal).FullName, query.Table.FullName);
        }

        [Test]
        public void Rerieve_all()
        {
            var query = session.SelectAll<Zoo>().Where(z => z.AcceptNewAnimals).EqualToValue(true);
            var zoos = query.FetchAll();
            Console.WriteLine("Found {0} zoos", zoos.Count);

            Assert.IsTrue(zoos.Count >= 4);
        }
    }
}

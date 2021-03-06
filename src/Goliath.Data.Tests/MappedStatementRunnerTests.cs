﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Goliath.Data.Mapping;
using Goliath.Data.Providers.Sqlite;
using Goliath.Data.Utils;
using Goliath.Data.DataAccess;
using WebZoo.Data;
namespace Goliath.Data.Tests
{
    [TestFixture]
    public class MappedStatementRunnerTests
    {
        [Test]
        public void RunStatement_null_session_should_throw()
        {
            MappedStatementRunner runner = new MappedStatementRunner();
            Assert.Throws<ArgumentNullException>(() => runner.RunStatement<int>(null, "countZooStatement"));
        }

        [Test]
        public void RunStatement_session_trying_to_run_non_query_should_throw_when_picking_wrong_method()
        {
            var session = SessionHelper.Factory.OpenSession();
            MappedStatementRunner runner = new MappedStatementRunner();
            Assert.Throws<GoliathDataException>(() => runner.RunStatement<int>(session, "insertZoos", null, new Zoo()
            {
                Name = "zooblar",
                AcceptNewAnimals = true,
                City = "Kosovo"
            }, new Zoo() { Name = "Trenton", City = "Trenton", AcceptNewAnimals = false }));
        }

        [Test]
        public void RunStatement_Query_entity_mapped_statement()
        {
            var session = SessionHelper.Factory.OpenSession();
            string statName = StatementStore.BuildMappedStatementName(typeof(Zoo), MappedStatementType.Query);
            Console.WriteLine("Statement name {0}", statName);
            Zoo sdZoo = new Zoo() { Name = "SD Zoo", City = "San Diego", AcceptNewAnimals = true };

            MappedStatementRunner runner = new MappedStatementRunner();
            var verify = runner.RunStatement<Zoo>(session, statName, null, sdZoo);

            Assert.AreEqual("SD Zoo", verify.Name);
            Assert.IsTrue(verify.Id > 0);
        }
    }
}

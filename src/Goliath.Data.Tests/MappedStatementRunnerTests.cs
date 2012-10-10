using System;
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
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void RunStatement_null_session_should_throw()
        {
            try
            {
                MappedStatementRunner runner = new MappedStatementRunner();
                runner.RunStatement<int>(null, "countZooStatement");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            Assert.Fail("The session variable was null, it should have thrown an exception.");
        }

        [Test, ExpectedException(typeof(GoliathDataException))]
        public void RunStatement_null_session_trying_to_run_non_query_should_throw_when_picking_wrong_method()
        {
            try
            {
                var session = SessionHelper.Factory.OpenSession();
                MappedStatementRunner runner = new MappedStatementRunner();
                runner.RunStatement<int>(session, "insertZoos", null, new Zoo() { Name = "zooblar", AcceptNewAnimals = true, City = "Kosovo" }, new Zoo() { Name = "Trenton", City = "Trenton", AcceptNewAnimals = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            Assert.Fail("insertZoos statement is non query.");
        }

        [Test]
        public void RunStatement_Query_entity_mapped_statement()
        {
            var session = SessionHelper.Factory.OpenSession();
            string statName = StatementStore.BuildProcedureName(typeof(Zoo), MappedStatementType.Query);
            Console.WriteLine("Statement name {0}", statName);
            Zoo sdZoo = new Zoo() { Name = "SD Zoo", City = "San Diego", AcceptNewAnimals = true };

            MappedStatementRunner runner = new MappedStatementRunner();
            var verify = runner.RunStatement<Zoo>(session, statName, null, sdZoo);

            Assert.AreEqual("SD Zoo", verify.Name);
            Assert.IsTrue(verify.Id > 0);
        }
    }
}

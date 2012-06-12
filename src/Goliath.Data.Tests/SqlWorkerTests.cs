using System;
using NUnit.Framework;
using WebZoo.Data;

namespace Goliath.Data.Tests
{

    [TestFixture]
    public class SqlWorkerTests
    {
        [Test]
        public void BuildDeleteSql_do_not_cascade_only_one_entity_should_be_deleted()
        {
            var session = SessionHelper.Factory.OpenSession();
            var sqlWorker = session.SessionFactory.DataSerializer.CreateSqlWorker();

            Zoo myZoo = new Zoo() { Id = 1, AcceptNewAnimals = true, City = "Kinshasa", Name = "Kinshasa Zoo" };
            var entMap = SessionHelper.Factory.DbSettings.Map.GetEntityMap(typeof(Zoo).FullName);

            var batchOps = sqlWorker.BuildDeleteSql<Zoo>(entMap, myZoo, false);

            Assert.AreEqual(1, batchOps.Operations.Count);

            string commtxt = batchOps.Operations[0].SqlText;
            Console.WriteLine(commtxt);

            Assert.AreEqual("DELETE FROM zoos WHERE zoos.Id = $zoo_Id_0", commtxt);
        }

        [Test]
        public void BuildDeleteSql_cascade_delete_should_have_parent_deleted_too_in_the_statement()
        {
            var session = SessionHelper.Factory.OpenSession();
            var sqlWorker = session.SessionFactory.DataSerializer.CreateSqlWorker();

            Monkey monkey = new Monkey()
            {
                Age = 2,
                CanDoTricks = false,
                Family = "don't know",
                Id = 1,
                Location = "New York",
                Name = "Curious George"
            };

            var entMap = SessionHelper.Factory.DbSettings.Map.GetEntityMap(typeof(Monkey).FullName);
            var batchOps = sqlWorker.BuildDeleteSql<Monkey>(entMap, monkey, false);

            Assert.AreEqual(1, batchOps.Operations.Count);
            Assert.AreEqual(1, batchOps.SubOperations.Count);

            Console.WriteLine(batchOps.Operations[0].SqlText);
            Console.WriteLine(batchOps.SubOperations[0].Operations[0].SqlText);

            Assert.AreEqual("DELETE FROM monkeys WHERE monkeys.Id = $monk_Id_0", batchOps.Operations[0].SqlText);
            Assert.AreEqual("DELETE FROM animals WHERE animals.Id = $anim_Id_0", batchOps.SubOperations[0].Operations[0].SqlText);
        }
    }
}

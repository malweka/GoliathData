using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Sql;
using NUnit.Framework;
using WebZoo.Data;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class InsertSqlBuilderTests
    {
        [Test]
        public void Build_insert_statements_for_simple_entity_without_relation_should_create_one_insert_statement()
        {
            //insert a zoo
            var zooEntMap = SessionHelper.Factory.DbSettings.Map.EntityConfigs.FirstOrDefault(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal));

            Zoo zoo = new Zoo() { AcceptNewAnimals = true, Name = Guid.NewGuid().ToString(), City = "kaaow" };
            var session = SessionHelper.Factory.OpenSession();
            InsertSqlBuilder builder = new InsertSqlBuilder();
            var execList = builder.Build(zoo, zooEntMap, session);

            Assert.AreEqual(1, execList.Statements.Count);
            Assert.IsTrue(execList.Statements[0].Processed);

            Assert.IsTrue(zoo.Id > 0);
        }

        [Test]
        public void Build_insert_entity_check_that_it_generate_parent_sql_first_inheritance()
        {
            var monkEntMap = SessionHelper.Factory.DbSettings.Map.EntityConfigs.FirstOrDefault(c => string.Equals(c.Name, "Monkey", StringComparison.Ordinal));

            Monkey monkey = new Monkey()
            {
                Age = 12,
                CanDoTricks = true,
                Family = "Singe",
                Location = "somecage",
                Name = "joemonkey",
                ReceivedOn = DateTime.Now,
                ZooId = 1
            };

            var session = SessionHelper.Factory.OpenSession();
            InsertSqlBuilder builder = new InsertSqlBuilder();
            var execList = builder.Build(monkey, monkEntMap, session);

            Assert.AreEqual(2, execList.Statements.Count);
            Assert.IsTrue(execList.Statements[1].Processed);

            Assert.IsTrue(monkey.Id > 0);
        }

        [Test]
        public void Build_insert_entity_with_new_entity_in_many2one_relation()
        {
            var monkEntMap = SessionHelper.Factory.DbSettings.Map.EntityConfigs.FirstOrDefault(c => string.Equals(c.Name, "Monkey", StringComparison.Ordinal));

            Monkey monkey = new Monkey()
            {
                Age = 12,
                CanDoTricks = true,
                Family = "Singe",
                Location = "cage2344",
                Name = "smart monkey",
                ReceivedOn = DateTime.Now,
                Zoo = new Zoo() { AcceptNewAnimals = true, Name = Guid.NewGuid().ToString(), City = "sfdfsdfsfe" },
            };

            var session = SessionHelper.Factory.OpenSession();
            InsertSqlBuilder builder = new InsertSqlBuilder();
            var execList = builder.Build(monkey, monkEntMap, session);

            Assert.AreEqual(3, execList.Statements.Count);
            Assert.IsTrue(execList.Statements[2].Processed);

            Assert.IsTrue(monkey.Id > 0);
            Assert.IsTrue(monkey.Zoo.Id > 0);
        }

        //TODO" use NUnit TestCase to test with both sqlite and sql server
        [Test]
        public void Build_insert_entity_with_existing_entity_in_many2one_relation()
        {
            var monkEntMap = SessionHelper.Factory.DbSettings.Map.EntityConfigs.FirstOrDefault(c => string.Equals(c.Name, "Monkey", StringComparison.Ordinal));
            var zooEntMap = SessionHelper.Factory.DbSettings.Map.EntityConfigs.FirstOrDefault(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal));

            var session = SessionHelper.Factory.OpenSession();
            InsertSqlBuilder builder = new InsertSqlBuilder();

            Zoo zoo = new Zoo() { AcceptNewAnimals = true, Name = Guid.NewGuid().ToString(), City = "exwee" };
            builder.Build(zoo, zooEntMap, session);

            Assert.IsTrue(zoo.Id > 0);

            Monkey monkey = new Monkey()
            {
                Age = 12,
                CanDoTricks = true,
                Family = "Singe",
                Location = "cage 48875",
                Name = "curious george",
                ReceivedOn = DateTime.Now,
                Zoo = zoo,
            };
            Console.WriteLine("---- Building monkey ----");
            var execList = builder.Build(monkey, monkEntMap, session);

            Assert.AreEqual(2, execList.Statements.Count);
            Assert.IsTrue(execList.Statements[1].Processed);

            Assert.IsTrue(monkey.Id > 0);
            Assert.AreEqual(zoo, monkey.Zoo);
        }
        
        [Test]
        public void Build_many_to_many_relation_should_save_new_entities()
        {
            var session = SessionHelper.Factory.OpenSession();
            InsertSqlBuilder builder = new InsertSqlBuilder();

            Employee empl = new Employee { AssignedToZooId = 1, EmailAddress = "mail@com", FirstName = "Joe", HiredOn = DateTime.Now, LastName = "Smith", Title = "some edm", Telephone = "125699555" };

            Animal marsu = new Animal()
                               {
                                   Name = "marsupilami",
                                   Age = 4,
                                   ReceivedOn = DateTime.Now,
                                   Location = "marsu L23",
                                   ZooId = 1
                               };

            empl.AnimalsOnAnimalsHandler_EmployeeId.Add(marsu);
            var execList = builder.Build(empl, session);

            Assert.AreEqual(3, execList.Statements.Count);
            Assert.IsFalse(execList.Statements[2].Processed);
        }

        [Test]
        public void Build_many_to_many_inverse_is_false_should_not_create_inserts_for_relations()
        {
            var session = SessionHelper.Factory.OpenSession();
            InsertSqlBuilder builder = new InsertSqlBuilder();

            Employee empl = new Employee { AssignedToZooId = 1, EmailAddress = "mail@com", FirstName = "Joe", HiredOn = DateTime.Now, LastName = "Smith", Title = "some edm", Telephone = "125699555" };

            Animal marsu = new Animal()
            {
                Name = "marsupilami",
                Age = 4,
                ReceivedOn = DateTime.Now,
                Location = "marsu L23",
                ZooId = 1
            };

            marsu.EmployeesOnAnimalsHandler_AnimalId.Add(empl);

            var execList = builder.Build(marsu, session);
            Assert.AreEqual(1, execList.Statements.Count);
        }
    }
}

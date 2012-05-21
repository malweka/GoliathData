using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using System.Linq;

namespace Goliath.Data.Tests
{
    using Mapping;

    [TestFixture]
    public class SqlProcedureCollectionTests
    {

        [Test, ExpectedException(typeof(GoliathDataException))]
        public void Ctor_Unsupported_platform_should_throw_exception()
        {
            SqlProcedureStore col = new SqlProcedureStore("RamenNoodleDB");
            Assert.Fail("should have thrown an exception because RamenNoodleDB is not supported");
        }

        [Test]
        public void Add_EntityMap_should_build_correct_name()
        {
            SqlProcedureStore col = new SqlProcedureStore();
            EntityMap map = new EntityMap("Faker", "tb_faker");
            map.AssemblyName = "faker.dll";
            map.Namespace = "Test";
            col.Add(map, ProcedureType.Insert, "TEST");

            var proc = col.First();
            Assert.AreEqual("Test.Faker_Insert", proc.Name);
        }

        [Test]
        public void Add_with_name_db_name_respect_provided_name()
        {
            SqlProcedureStore col = new SqlProcedureStore();
            col.Add("fake_proc", "fake_proc_dbname", ProcedureType.ExecuteNonQuery, "TEST", RdbmsBackend.SupportedSystemNames.Sqlite3);
            var proc = col.First();

            Assert.AreEqual("fake_proc", proc.Name);
            Assert.AreEqual("fake_proc_dbname", proc.DbName);
        }

        [Test]
        public void Add_unsupported_rdbms_should_not_be_stored()
        {
            SqlProcedureStore col = new SqlProcedureStore(RdbmsBackend.SupportedSystemNames.Mssql2005);
            col.Add("fake_proc", "fake_proc_dbname", ProcedureType.ExecuteNonQuery, "TEST", RdbmsBackend.SupportedSystemNames.Sqlite3);

            Assert.AreEqual(0, col.InnerProcedureList.Count);
        }

        [Test]
        public void TryGetValue_get_proc_for_type_where_platform_is_supported_return_proc()
        {
            SqlProcedureStore col = new SqlProcedureStore(RdbmsBackend.SupportedSystemNames.Mssql2008);
            Type type = typeof(WebZoo.Data.Animal);

            EntityMap map = new EntityMap(type.Name, "tb_faker");
            map.AssemblyName = "faker.dll";
            map.Namespace = type.Namespace;

            col.Add(map, "fake", ProcedureType.Insert, "TEST", RdbmsBackend.SupportedSystemNames.Mssql2005);

            SqlProcedure proc;
            Assert.IsTrue(col.TryGetValue(type, ProcedureType.Insert, out proc));
            Assert.IsNotNull(proc);
        }

        [Test]
        public void TryGetValue_get_proc_for_type_where_platform_is_not_supported_return_null()
        {
            SqlProcedureStore col = new SqlProcedureStore(RdbmsBackend.SupportedSystemNames.Postgresql9);
            Type type = typeof(WebZoo.Data.Animal);

            EntityMap map = new EntityMap(type.Name, "tb_faker");
            map.AssemblyName = "faker.dll";
            map.Namespace = type.Namespace;

            col.Add(map, "fake", ProcedureType.Insert, "TEST", RdbmsBackend.SupportedSystemNames.Mssql2005);

            SqlProcedure proc;
            Assert.IsFalse(col.TryGetValue(type, ProcedureType.Insert, out proc));
            Assert.IsNull(proc);
        }
    }
}

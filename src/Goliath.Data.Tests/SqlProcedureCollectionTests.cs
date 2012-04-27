using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;

namespace Goliath.Data.Tests
{
    using Mapping;

    [TestFixture]
    public class SqlProcedureCollectionTests
    {

        [Test]
        public void Add_EntityMap_should_build_correct_name()
        {
            SqlProcedureCollection col = new SqlProcedureCollection();
            EntityMap map = new EntityMap("Faker", "tb_faker");
            map.AssemblyName="faker.dll";
            map.Namespace="Test";
            col.Add(map, ProcedureType.Insert, "TEST");

            var proc = col[0];
            Assert.AreEqual("Test.Faker_Insert", proc.Name);
        }

        [Test]
        public void Add_with_name_db_name_respect_provided_name()
        {
            SqlProcedureCollection col = new SqlProcedureCollection();
            col.Add("fake_proc", "fake_proc_dbname", ProcedureType.ExecuteNonQuery, "TEST", SupportedRdbms.All);
            var proc = col[0];

            Assert.AreEqual("fake_proc", proc.Name);
            Assert.AreEqual("fake_proc_dbname", proc.DbName);
        }

        [Test]
        public void TryGetValue_get_proc_for_type_where_platform_is_supported_return_proc()
        {
            SqlProcedureCollection col = new SqlProcedureCollection(SupportedRdbms.Mssql2008);
            Type type = typeof(WebZoo.Data.Animal);

            EntityMap map = new EntityMap(type.Name, "tb_faker");
            map.AssemblyName = "faker.dll";
            map.Namespace = type.Namespace;

            col.Add(map, "fake", ProcedureType.Insert, "TEST", SupportedRdbms.MssqlAll);

            SqlProcedure proc;
            Assert.IsTrue(col.TryGetValue(type, ProcedureType.Insert, out proc));
            Assert.IsNotNull(proc);
        }

        [Test]
        public void TryGetValue_get_proc_for_type_where_platform_is_not_supported_return_null()
        {
            SqlProcedureCollection col = new SqlProcedureCollection(SupportedRdbms.Postgresql9);
            Type type = typeof(WebZoo.Data.Animal);

            EntityMap map = new EntityMap(type.Name, "tb_faker");
            map.AssemblyName = "faker.dll";
            map.Namespace = type.Namespace;

            col.Add(map, "fake", ProcedureType.Insert, "TEST", SupportedRdbms.MssqlAll);

            SqlProcedure proc;
            Assert.IsFalse(col.TryGetValue(type, ProcedureType.Insert, out proc));
            Assert.IsNull(proc);
        }
    }
}

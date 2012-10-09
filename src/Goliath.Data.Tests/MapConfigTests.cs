using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Goliath.Data.Tests
{
    using Mapping;

    [TestFixture]
    public class MapConfigTests
    {
        [Test]
        public void Load_simple_xml_config_should_have_valid_map_config()
        {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test001.data.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            Assert.AreEqual(2, config.EntityConfigs.Count);
        }

        [Test, ExpectedException(typeof(MappingException))]
        public void Load_xml_config_with_entity_non_existing_map_should_throw()
        {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test002.data.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            Assert.Fail("should have thrown");
        }

        [Test]
        public void Load_conventions_statement_without_name_should_default_to_naming_convention()
        {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test_statement_valid.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            var zooInserts = config.UnprocessedStatements.Where(c => c.Name == "WebZoo.Data.Zoo_Insert").ToList();
            Assert.AreEqual(1, zooInserts.Count);
        }

        [Test]
        public void Load_conventions_statement_insert_update_without_InputParameterType_should_default_entity_result()
        {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test_statement_valid.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            var statement = config.UnprocessedStatements.Where(c => c.Name == "updateTest").First();
            Assert.AreEqual("WebZoo.Data.Zoo", statement.InputParametersMap.Values.First());

            statement = config.UnprocessedStatements.Where(c => c.Name == "testInsert").First();
            Assert.AreEqual("WebZoo.Data.Zoo", statement.InputParametersMap.Values.First());
        }

        [Test]
        public void Load_conventions_statement_insert_update_without_resul_map_should_default_integer_result()
        {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test_statement_valid.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            var statement = config.UnprocessedStatements.Where(c => c.Name == "updateTest").First();
            Assert.AreEqual(typeof(int).ToString(), statement.ResultMap);

            statement = config.UnprocessedStatements.Where(c => c.Name == "testInsert").First();
            Assert.AreEqual(typeof(int).ToString(), statement.ResultMap);
        }

        [Test]
        public void Load_conventions_statement_query_without_resul_map_should_default_type_of_entity_map_result()
        {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test_statement_valid.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            var statement = config.UnprocessedStatements.Where(c => c.Name == "fakequery").First();
            Assert.AreEqual("WebZoo.Data.Zoo", statement.ResultMap);
        }

        [Test]
        public void Load_valid_map_file_return_list_of_valid_statements()
        {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "TestFullMap.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            var statements = config.UnprocessedStatements.ToList();
            Assert.AreEqual(6, statements.Count);

            foreach (var st in statements)
            {
                Assert.AreEqual("WebZoo.Data.Zoo", st.DependsOnEntity);
            }

            var statementWithParams = statements.Where(s => s.Name == "updateTest2").First();
            Assert.AreEqual(3, statementWithParams.DBParametersMap.Count);
        }

        [Test, ExpectedException(typeof(MappingSerializationException))]
        public void Load_generic_statement_without_operation_type_should_throw()
        {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test_statement_invalid_statement_operationType.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            Assert.Fail("should have thrown an exception; map is invalid");
        }


        [Test, ExpectedException(typeof(MappingSerializationException))]
        public void Load_statement_not_depending_on_entity_with_no_name_should_throw()
        {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test_statement_invalid_missing_name.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            Assert.Fail("should have thrown an exception: Statements/query does not have a name.");
        }
    }
}

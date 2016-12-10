using System;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Linq;
using Goliath.Data.Providers;
using Goliath.Data.Sql;

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

		[Test]
		public void Load_xml_config_with_entity_non_existing_map_should_throw()
		{
			string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test002.data.xml");
			MapConfig config = new MapConfig();
		    Assert.Throws<MappingException>(() => config.Load(testMapfile));

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

			var statement = config.UnprocessedStatements.Where(c => c.Name == "updateTest").FirstOrDefault();
			Assert.AreEqual("WebZoo.Data.Zoo", statement.InputParametersMap.Values.First());

			statement = config.UnprocessedStatements.Where(c => c.Name == "testInsert").FirstOrDefault();
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
			Assert.AreEqual(3, statementWithParams.DbParametersMap.Count);
		}

		[Test]
		public void Load_reference_dbType_should_be_valid()
		{
			string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "TestFullMap.xml");
			MapConfig config = new MapConfig();
			config.Load(testMapfile);

			var ent = config.GetEntityMap("WebZoo.Data.Animal");
			var rel = ent.Relations["Zoo"];
			Assert.AreEqual(System.Data.DbType.Int32, rel.DbType);
		}

		[Test]
		public void Load_generic_statement_without_operation_type_should_throw()
		{
			string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test_statement_invalid_statement_operationType.xml");
			MapConfig config = new MapConfig();
            Assert.Throws<MappingSerializationException>(() => config.Load(testMapfile));
		}


		[Test]
		public void Load_statement_not_depending_on_entity_with_no_name_should_throw()
		{
			string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "Test_statement_invalid_missing_name.xml");
			MapConfig config = new MapConfig();
            Assert.Throws<MappingSerializationException>(() => config.Load(testMapfile));
        }

	    [Test]
	    public void Sort_test_if_map_is_sorted()
	    {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "sort_data.map.xml");
	        var config = MapConfig.Create(testMapfile, true, new FakeIdGenerator());

            //var sorter = new MapSorter();
	        //var sorted = sorter.Sort(config.EntityConfigs);
	        config.Sort();

            config.Save(@"C:\junk\sortedmap.xml", true);

	    }
	}

    class FakeIdGenerator : IKeyGenerator<long>
    {
        public long Generate(SqlDialect sqlDialect, EntityMap entityMap, string propertyName, out SqlOperationPriority priority)
        {
            Random r = new Random();
            priority = SqlOperationPriority.Low;

            return r.Next(1, 999999999);
        }

        public Type KeyType => typeof(long);
        public string Name => "Cms_Integer_keyGen";
        public object GenerateKey(SqlDialect sqlDialect, EntityMap entityMap, string propertyName, out SqlOperationPriority priority)
        {
            Random r = new Random();
            priority = SqlOperationPriority.Low;

            return r.Next(1, 999999999);
        }

        public bool IsDatabaseGenerated { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Linq;
using Goliath.Data.Entity;

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
	}

	[TestFixture]
	public class ChangeTrackerTests
	{
		[Test]
		public void Changed_boolean_TrackableShould_be_dirty()
		{
			var m = new ChangeModelTester(false);

			m.IsBool = true;
			m.ChangeTracker.Init();
			m.ChangeTracker.LoadInitialValue("IsBool", true);
			m.ChangeTracker.Start();
			m.IsBool = false;

			Assert.IsTrue(m.IsDirty);
		}

		[Test]
		public void no_Change_boolean_Trackable_Should_not_be_dirty()
		{
			var m = new ChangeModelTester(false);

			m.IsBool = true;
			m.ChangeTracker.Init();
			m.ChangeTracker.LoadInitialValue("IsBool", true);
			m.ChangeTracker.Start();
			m.IsBool = true;

			Assert.IsFalse(m.IsDirty);
		}


		[Test]
		public void Changed_null_boolean_TrackableShould_be_dirty()
		{
			var m = new ChangeModelTester(false);

			m.IsNullableBool = true;
			m.ChangeTracker.Init();
			m.ChangeTracker.LoadInitialValue("IsNullableBool", true);
			m.ChangeTracker.Start();
			m.IsNullableBool = false;

			Assert.IsTrue(m.IsDirty);
		}

		[Test]
		public void no_Change_nullable_boolean_Trackable_Should_not_be_dirty()
		{
			var m = new ChangeModelTester(false);

			m.IsNullableBool = true;
			m.ChangeTracker.Init();
			m.ChangeTracker.LoadInitialValue("IsNullableBool", true);
			m.ChangeTracker.Start();
			m.IsNullableBool = true;

			Assert.IsFalse(m.IsDirty);
		}

		[Test]
		public void Changed_date_TrackableShould_be_dirty()
		{
			var m = new ChangeModelTester(false);

			var initVal = DateTime.Now;

			m.TheDateTime = initVal;
			m.ChangeTracker.Init();
			m.ChangeTracker.LoadInitialValue("TheDateTime", initVal);
			m.ChangeTracker.Start();
			m.TheDateTime = DateTime.Now.AddMinutes(5);

			Assert.IsTrue(m.IsDirty);
		}

		[Test]
		public void no_Change__date_Trackable_Should_not_be_dirty()
		{
			var m = new ChangeModelTester(false);
			var initVal = DateTime.Now;
			m.TheDateTime = initVal;
			m.ChangeTracker.Init();
			m.ChangeTracker.LoadInitialValue("TheDateTime", initVal);
			m.ChangeTracker.Start();
			m.TheDateTime = initVal;

			Assert.IsFalse(m.IsDirty);
		}

		[Test]
		public void Changed_int_TrackableShould_be_dirty()
		{
			var m = new ChangeModelTester(false);

			var initVal = 2;

			m.Dummy = initVal;
			m.ChangeTracker.Init();
			m.ChangeTracker.LoadInitialValue("Dummy", initVal);
			m.ChangeTracker.Start();
			m.Dummy = 5;

			Assert.IsTrue(m.IsDirty);
		}

		[Test]
		public void no_Change__int_Trackable_Should_not_be_dirty()
		{
			var m = new ChangeModelTester(false);
			var initVal = 2;
			m.Dummy = initVal;
			m.ChangeTracker.Init();
			m.ChangeTracker.LoadInitialValue("Dummy", initVal);
			m.ChangeTracker.Start();
			m.Dummy = initVal;

			Assert.IsFalse(m.IsDirty);
		}

	}

	class ChangeModelTester : ITrackable
	{
		private bool isbool;

		public bool IsBool
		{
			get { return isbool; }
			set
			{
				isbool = value;
				NotifyChange("IsBool", value);
			}
		}

		private bool? nullableBool;

		public bool? IsNullableBool
		{
			get
			{
				return nullableBool;
			}
			set
			{
				nullableBool = value;
				NotifyChange("IsNullableBool", value);
			}
		}

		private DateTime dd;

		public DateTime TheDateTime
		{
			get { return dd; }
			set
			{
				dd = value;
				NotifyChange("TheDateTime", value);
			}
		}

		private int dum;

		public int Dummy
		{
			get
			{
				return dum;
			}
			set
			{
				dum = value;
				NotifyChange("Dummy", value);
			}
		}

		readonly IChangeTracker changeTracker;

		protected void NotifyChange(string propName, object value)
		{
			ChangeTracker.Track(propName, value);
		}

		public bool IsDirty
		{
			get
			{
				return ChangeTracker != null && ChangeTracker.HasChanges;
			}
		}
		public IChangeTracker ChangeTracker
		{
			get { return changeTracker; }
		}

		public long Version { get; set; }

		static IDictionary<string, object> LoadInitialValues()
		{
			var initValues = new Dictionary<string, object>
			{
				{"IsBool", false},
				{"IsNullableBool", default(bool?)},
				{"TheDateTime", default(DateTime)},
				{"Dummy", default(int)},
			};

			return initValues;
		}

		public ChangeModelTester(bool startTrackingOnInit)
		{
			changeTracker = new ChangeTracker(LoadInitialValues);
			if (!startTrackingOnInit) return;
			changeTracker.Init();
			changeTracker.Start();
			Version = changeTracker.Version;
		}
	}
}

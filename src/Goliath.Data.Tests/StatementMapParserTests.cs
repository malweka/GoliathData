using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Goliath.Data.Tests
{
    using Mapping;
    using Providers.Sqlite;
    using Utils;

    [TestFixture]
    public class StatementMapParserTests
    {
        [Test]
        public void Parse_valid_string_should_return_parsed_text()
        {
            string template = "INSERT INTO @{TableName}(@{col:Name},@{col:City},@{col:AcceptNewAnimals}) VALUES(@{prop:Name},@{prop:City},@{prop:AcceptNewAnimals})";
            string compiled = "INSERT INTO zoos(Name,City,AcceptNewAnimals) VALUES($Name,$City,$AcceptNewAnimals)";
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "TestFullMap.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            var zooEntMap = config.EntityConfigs.Where(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal))
                    .FirstOrDefault();

            StatementMapParser parser = new StatementMapParser();
            var statement = parser.Parse(new SqliteDialect(), zooEntMap, template);

            Assert.AreEqual(compiled, statement.Body);
            Assert.AreEqual(3, statement.ParamPropertyMap.Count);
        }

        [Test, ExpectedException(typeof(GoliathDataException))]
        public void Parse_not_supported_entityMap_property_should_throw()
        {
            string template = "INSERT INTO @{AssemblyName}(@{col:Name},@{col:City},@{col:AcceptNewAnimals}) VALUES(@{prop:Name},@{prop:City},@{prop:AcceptNewAnimals})";

            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "TestFullMap.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            var zooEntMap = config.EntityConfigs.Where(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal))
                    .FirstOrDefault();

            StatementMapParser parser = new StatementMapParser();
            var statement = parser.Parse(new SqliteDialect(), zooEntMap, template);

            Assert.Fail("Should have thrown, AssemblyName property is not supported ");
        }

        [Test, ExpectedException(typeof(GoliathDataException))]
        public void Parse_non_existing_column_should_throw()
        {
            string template = "INSERT INTO @{TableName}(@{col:WaterPressure},@{col:City},@{col:AcceptNewAnimals}) VALUES(@{prop:Name},@{prop:City},@{prop:AcceptNewAnimals})";
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "TestFullMap.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            var zooEntMap = config.EntityConfigs.Where(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal))
                    .FirstOrDefault();

            StatementMapParser parser = new StatementMapParser();
            var statement = parser.Parse(new SqliteDialect(), zooEntMap, template);

            Assert.Fail("Should have thrown, column WaterPressure doesn't exist");
        }

        [Test]
        public void Parse_with_several_input_parameters()
        {
            string template = "select @{col:a.Id}, @{sel:a.Name}, @{sel:a.City}, @{sel:a.AcceptNewAnimals} from  @{a.TableName} where @{prop:a.Id} = @{prop:b.Id}";
            string verify = "select Id, Name as zoo_Name, City as zoo_City, AcceptNewAnimals as zoo_AcceptNewAnimals from  zoos where $a_Id = $b_Id";
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "TestFullMap.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            StatementMapParser parser = new StatementMapParser();
            Dictionary<string, StatemenInputParam> inputParams = new Dictionary<string, StatemenInputParam> { { "a", new StatemenInputParam() { Name = "a", Type = "WebZoo.Data.Zoo" } }, { "b", new StatemenInputParam() { Name = "b", Type = "WebZoo.Data.Animal" } } };

            var statement = parser.Parse(new SqliteDialect(), config, inputParams, template);
            Assert.AreEqual(verify, statement.Body);

        }
    }
}

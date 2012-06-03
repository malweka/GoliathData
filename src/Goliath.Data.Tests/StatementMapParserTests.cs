using System;
using System.IO;
using System.Linq;
using MbUnit.Framework;

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
            var statement = parser.Parse(new SqliteSqlMapper(), zooEntMap, template);

            Assert.AreEqual<string>(compiled, statement.Body);
            Assert.AreEqual<int>(3, statement.ParamPropertyMap.Count);
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
            var statement = parser.Parse(new SqliteSqlMapper(), zooEntMap, template);

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
            var statement = parser.Parse(new SqliteSqlMapper(), zooEntMap, template);

            Assert.Fail("Should have thrown, column WaterPressure doesn't exist");
        }
    }
}

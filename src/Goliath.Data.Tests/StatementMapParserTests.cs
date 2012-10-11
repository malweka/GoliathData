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
            string template = "INSERT INTO @{TableName}(@{sel:Name},@{sel:City},@{sel:AcceptNewAnimals}) VALUES(@{prop:Name},@{prop:City},@{prop:AcceptNewAnimals})";
            string compiled = "INSERT INTO zoos(Name as zoo_Name,City as zoo_City,AcceptNewAnimals as zoo_AcceptNewAnimals) VALUES($Name,$City,$AcceptNewAnimals)";
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "TestFullMap.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            var zooEntMap = config.EntityConfigs.Where(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal))
                    .FirstOrDefault();

            StatementMapParser parser = new StatementMapParser();
            StatementInputParam inputParam = new StatementInputParam() { Name = "a", ClrType = typeof(WebZoo.Data.Zoo), Type = zooEntMap.FullName, Map = zooEntMap, IsMapped = true };
            var statement = parser.Parse(new SqliteDialect(), zooEntMap, template, inputParam);
            Console.WriteLine(statement.Body);
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
            var statement = parser.Parse(new SqliteDialect(), zooEntMap, template, null);

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
            var statement = parser.Parse(new SqliteDialect(), zooEntMap, template, null);

            Assert.Fail("Should have thrown, column WaterPressure doesn't exist");
        }

        [Test]
        public void Parse_with_several_input_parameters()
        {
            string template = "select @{sel:a.Id}, @{sel:a.Name}, @{sel:a.City}, @{sel:a.AcceptNewAnimals} from  @{a.TableName} where @{prop:a.Id} = @{prop:b.Id}";
            string verify = "select Id as zoo_Id, Name as zoo_Name, City as zoo_City, AcceptNewAnimals as zoo_AcceptNewAnimals from  zoos where $a_Id = $b_Id";
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MapConfigTests", "TestFullMap.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            StatementMapParser parser = new StatementMapParser();
            Dictionary<string, StatementInputParam> inputParams = new Dictionary<string, StatementInputParam> { { "a", new StatementInputParam() { Name = "a", Type = "WebZoo.Data.Zoo", ClrType=typeof(WebZoo.Data.Zoo) } }, 
            { "b", new StatementInputParam() { Name = "b", Type = "WebZoo.Data.Animal", ClrType=typeof(WebZoo.Data.Animal)} } };

            var statement = parser.Parse(new SqliteDialect(), config, inputParams, template);
            Console.WriteLine(statement.Body);
            Assert.AreEqual(verify, statement.Body);
        }

        [Test]
        public void Parse_with_db_parameters_parse_parameters()
        {
            string verify = "select  * from zoos where Name = $p1 and AcceptAnimals = $p2;";
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MappedStatementTests", "TestFullMap.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            Providers.Sqlite.SqliteProvider provider = new Providers.Sqlite.SqliteProvider();
            config.MapStatements(provider.Name);
            StatementMap statement;
            config.MappedStatements.TryGetValue("querySupergloo", out statement);
            StatementMapParser parser = new StatementMapParser();
            var compiled = parser.Parse(new SqliteDialect(), config, null, statement.Body, new QueryParam("p1"), new QueryParam("p2"));
            Console.WriteLine(compiled.Body.Trim());
            Assert.AreEqual(verify, compiled.Body.Trim());
        }

        [Test]
        public void Parse_input_parameters_and_add_db_parameter_names_to_resulting_compiled_statement()
        {
            string verify = "INSERT INTO zoos(name, city, acceptanimals) VALUES($a_Name, $a_City, $a_AcceptAnimals);\nINSERT INTO zoos(name, city, acceptanimals) VALUES($b_Name, $b_City, $b_AcceptAnimals);";

            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "MappedStatementTests", "TestFullMap.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            Providers.Sqlite.SqliteProvider provider = new Providers.Sqlite.SqliteProvider();
            config.MapStatements(provider.Name);
            StatementMap statement;
            config.MappedStatements.TryGetValue("insertZoos", out statement);
            StatementMapParser parser = new StatementMapParser();

            Dictionary<string, StatementInputParam> inputParams = new Dictionary<string, StatementInputParam> { { "a", new StatementInputParam() { Name = "a", Type = "WebZoo.Data.Zoo", ClrType=typeof(WebZoo.Data.Zoo) } }, 
            { "b", new StatementInputParam() { Name = "b", Type = "WebZoo.Data.Zoo", ClrType=typeof(WebZoo.Data.Zoo)} } };

            var compiled = parser.Parse(new SqliteDialect(), config, inputParams, statement.Body.Trim());
            string[] verifySplit = verify.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] bodySplit = compiled.Body.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            Console.WriteLine(bodySplit[0].Trim());
            Assert.AreEqual(verifySplit[0], bodySplit[0].Trim());

            Console.WriteLine(bodySplit[1].Trim());
            Assert.AreEqual(verifySplit[1], bodySplit[1].Trim());

            Assert.AreEqual(6, compiled.ParamPropertyMap.Count);
            Assert.IsTrue(compiled.ParamPropertyMap.Values.Where(c => c.QueryParamName == "a_Name").FirstOrDefault() != null);
            Assert.IsTrue(compiled.ParamPropertyMap.Values.Where(c => c.QueryParamName == "b_AcceptAnimals").FirstOrDefault() != null);
        }
    }
}

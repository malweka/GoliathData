using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using System.Linq;
using Goliath.Data.DataAccess;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class EntitySerializerFactoryTests
    {
        [Test]
        public void SerializeEntity_should_entity()
        {
            var dbAccess = new Providers.Sqlite.SqliteDataAccess(@"Data Source=C:\Junk\Goliath.Data\WebZoo.db; Version=3");
            using (var conn = dbAccess.CreateNewConnection())
            {
                conn.Open();
                var map = Config.ConfigManager.CurrentSettings.Map;
                string sqlQuery = @"select zoo.Id as zoo_Id, zoo.Name as zoo_Name, zoo.City as zoo_City, zoo.AcceptNewAnimals as zoo_AcceptNewAnimals 
from zoos zoo";
                var dataReader = dbAccess.ExecuteReader(conn, sqlQuery);

                var ent = map.EntityConfigs.Where(c => string.Equals(c.Name, "Zoo", StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();
                dataReader.Read();

                EntitySerializerFactory serializer = new EntitySerializerFactory();
                var zoo = serializer.Serialize<WebZoo.Data.Sqlite.Zoo>(dataReader, ent);
                Assert.IsNotNull(zoo);
                dataReader.Dispose();
            }
        }
    }
}

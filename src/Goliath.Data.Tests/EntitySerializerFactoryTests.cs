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
            var dbConnector = new Providers.Sqlite.SqliteDbConnector(@"Data Source=C:\Junk\Goliath.Data\WebZoo.db; Version=3");
            var dbAccess = new DbAccess(dbConnector);

            using (var conn = dbConnector.CreateNewConnection())
            {
                conn.Open();
                var map = Config.ConfigManager.CurrentSettings.Map;
                //string sqlQuery = @"select zoo.Id as zoo_Id, zoo.Name as zoo_Name, zoo.City as zoo_City, zoo.AcceptNewAnimals as zoo_AcceptNewAnimals 
//from zoos zoo";

                string sqlQuery = @"select ani1.ZooId as ani1_ZooId, ani1.Id as ani1_Id, ani1.Name as ani1_Name, ani1.Age as ani1_Age, ani1.Location as ani1_Location, ani1.ReceivedOn as ani1_ReceivedOn  from animals ani1";

                var dataReader = dbAccess.ExecuteReader(conn, sqlQuery);

                var ent = map.EntityConfigs.Where(c => string.Equals(c.Name, "Animal", StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();
                //dataReader.Read();

                EntitySerializerFactory serializer = new EntitySerializerFactory();
                var zoo = serializer.Serialize<WebZoo.Data.Sqlite.Animal>(dataReader, ent);
                Assert.IsNotNull(zoo);
                dataReader.Dispose();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Providers;
using Goliath.Data.Providers.Sqlite;
using Goliath.Data.Providers.SqlServer;
using Goliath.Data.Generators;
using System.IO;
using Goliath.Data.Mapping;
using Goliath.Data.Transformers;
using Goliath.Data.DataAccess;
using Goliath.Data.Sql;


namespace Goliath.Data
{
    class Program
    {
        const string MapFileName = "GoData.Map.xml";

        static void Main(string[] args)
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.LastIndexOf("bin"));
            string sqlServerWorkingDirectory = Path.Combine(currentDir, "Generated", "Mssql2008");
            string sqliteWorkingDirectory = Path.Combine(currentDir, "Generated", "Sqlite");

            Console.WriteLine("Start run");

            Sqlite(sqliteWorkingDirectory);
            SqlServer(sqlServerWorkingDirectory);

            string templatePath = currentDir;

            Generate(sqlServerWorkingDirectory, templatePath);
            Generate(sqliteWorkingDirectory, templatePath);

            //QueryTest(sqliteWorkingDirectory);
            Console.WriteLine("done");
            //Console.ReadKey();
        }

        static void Generate(string workingFolder, string templateFolder)
        {
            string mapfile = Path.Combine(workingFolder, MapFileName);
            MapConfig project = MapConfig.Create(mapfile);
            var basefolder = workingFolder;
            ICodeGenerator generator = new RazorCodeGenerator();
            foreach (var table in project.EntityConfigs)
            {
                if (table.IsLinkTable)
                    continue;

                //try
                //{
                string fname = Path.Combine(basefolder, table.Name + ".cs");
                generator.Generate(Path.Combine(templateFolder, "Class.razt"), fname, table);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.ToString());
                //}
            }

            //foreach (var table in project.EntityConfigs)
            //{
            //    if (table.IsLinkTable)
            //        continue;
            //    //try
            //    //{
            //    string fname = Path.Combine(basefolder, table.Name + "Adapter.cs");
            //    generator.Generate(Path.Combine(templateFolder, "DataAdapter.razt"), fname, table);
            //    //}
            //    //catch (Exception ex)
            //    //{
            //    //    Console.WriteLine(ex.ToString());
            //    //}
            //}

            //generator.Generate(Path.Combine(templateFolder, "DataAdapterFactory.razt"),
            //    Path.Combine(basefolder, "DataAccessAdapterFactory.cs"),
            //    project);
        }

        static void Sqlite(string workingFolder)
        {
            ProjectSettings settings = new ProjectSettings();
            string pdir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin"));
            string dbfile = Path.Combine(pdir, "Data", "WebZoo.db");
            settings.ConnectionString = string.Format("Data Source={0}; Version=3", dbfile);
            settings.Namespace = "WebZoo.Data.Sqlite";
            settings.Version = "1.0";
            settings.AssemblyName = "WebZoo.Data";
            settings.BaseModel = "WebZoo.Data.BaseEntity";

            SqlMapper mapper = new SqliteSqlMapper();
            IDbConnector dbConnector = new SqliteDbConnector(settings.ConnectionString);
            IDbAccess db = new DbAccess(dbConnector);

            using (ISchemaDescriptor schema = new SqliteSchemaDescriptor(db, dbConnector, mapper, settings))
            {
                schema.ProjectSettings = settings;
                DataModelGenerator generator = new DataModelGenerator(schema, new NameTransformerFactory(settings), new DefaultTableNameAbbreviator());
                ComplexType baseModel = new ComplexType("WebZoo.Data.BaseEntity");
                baseModel.Properties.Add(new Property("Id", "Id", System.Data.DbType.Guid)
                {
                    ClrType = typeof(Guid),
                    IsPrimaryKey = true,
                    IsUnique = true,
                });
                MapConfig builder = generator.GenerateMap(settings, baseModel);
                CreateFolderIfNotExist(workingFolder);
                string mapfile = Path.Combine(workingFolder, MapFileName);
                builder.Save(mapfile, true);
            }
        }

        static void SqlServer(string workingFolder)
        {
            ProjectSettings settings = new ProjectSettings();

            settings.ConnectionString = "Data Source=localhost;Initial Catalog=DbZoo;Integrated Security=True";
            //settings.TablePrefixes = "Go";
            //settings.ConnectionString = "Data Source=localhost;Initial Catalog=GoliathDbGenerated;Integrated Security=True";
            settings.Namespace = "WebZoo.Data.SqlServer";
            settings.Version = "1.0";
            settings.AssemblyName = "WebZoo.Data";
            settings.BaseModel = "WebZoo.Data.BaseEntity";
            IDbConnector dbConnector = new MssqlDbConnector(settings.ConnectionString);
            IDbAccess db = new DbAccess(dbConnector);

            SqlMapper mapper = new Mssq2008SqlMapper();

            using (ISchemaDescriptor schemaDescriptor = new MssqlSchemaDescriptor(db, dbConnector, mapper, settings))
            {
                schemaDescriptor.ProjectSettings = settings;
                DataModelGenerator generator = new DataModelGenerator(schemaDescriptor, new NameTransformerFactory(settings), new DefaultTableNameAbbreviator());
                ComplexType baseModel = new ComplexType("WebZoo.Data.BaseEntity");
                baseModel.Properties.Add(new Property("Id", "Id", System.Data.DbType.Guid)
                {
                    ClrType = typeof(Guid),
                    IsPrimaryKey = true,
                    IsUnique = true,
                });

                MapConfig builder = generator.GenerateMap(settings, baseModel);
                CreateFolderIfNotExist(workingFolder);
                string mapfile = Path.Combine(workingFolder, MapFileName);
                builder.Save(mapfile, true);
            }
        }

        static void CreateFolderIfNotExist(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        static void QueryTest(string workingFolder)
        {

            //string sf = "/Users/hamsman/development";
            string pdir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin"));
			string dbfile = Path.Combine(pdir, "Data", "WebZoo.db");
            string cs = string.Format("Data Source={0}; Version=3", dbfile);
            
            string mapfile = Path.Combine(workingFolder, MapFileName);
            var sessionFactory = new Database().Configure(mapfile, cs)
                .Provider(new SqliteProvider()).Init();

            var sess = sessionFactory.OpenSession();
            MapConfig map = MapConfig.Create(mapfile);

            //var dbConnector = new Providers.SqlServer.MssqlDbConnector("Data Source=localhost;Initial Catalog=DbZoo;Integrated Security=True");
            var dbConnector = new Providers.Sqlite.SqliteDbConnector(cs);
			var dbAccess = new DbAccess(dbConnector);
            

            using (var conn = dbConnector.CreateNewConnection())
            {
                conn.Open();
//                string zooQuery = @"select zoo.Id as zoo_Id, zoo.Name as zoo_Name, zoo.City as zoo_City, zoo.AcceptNewAnimals as zoo_AcceptNewAnimals 
//                from zoos zoo";

//                string sqlQuery = @"select ani1.ZooId as ani1_ZooId, ani1.Id as ani1_Id, ani1.Name as ani1_Name, ani1.Age as ani1_Age, ani1.Location as ani1_Location, ani1.ReceivedOn as ani1_ReceivedOn  from animals ani1";
                SqliteSqlMapper mapper = new SqliteSqlMapper();               

                var animalEntMap = map.EntityConfigs.Where(c => string.Equals(c.Name, "Animal", StringComparison.Ordinal))
                    .FirstOrDefault();

                var zooEntMap = map.EntityConfigs.Where(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal))
                    .FirstOrDefault();

                SelectSqlBuilder selectBuilder = new SelectSqlBuilder(mapper, animalEntMap)
                .Where(new WhereStatement("Name").Equals("@Name"));
                string sstring = selectBuilder.Build();

                QueryParam qp = new QueryParam(string.Format("{0}{1}", zooEntMap.TableAbbreviation, "Id")) { Value = "CAF24C81-C7A1-4B5F-8CDA-D85D8ED5F2AF" };

                SelectSqlBuilder sqlBuilder = new SelectSqlBuilder(mapper, zooEntMap)
                   .Where(new WhereStatement(string.Format("{0}.{1}", zooEntMap.TableAbbreviation, "Id"))
                            .Equals(mapper.CreateParameterName(qp.Name)));

                QueryInfo qInfo = new QueryInfo();
                qInfo.QuerySqlText = sqlBuilder.Build();
                qInfo.Parameters = new QueryParam[] { qp };
                System.Data.Common.DbParameter[]  px = dbAccess.CreateParameters(qInfo.Parameters).ToArray();
                var xreader = dbAccess.ExecuteReader(conn, qInfo.QuerySqlText, px);
                Console.WriteLine(xreader.HasRows);

                //dataReader.Read();
                //Providers.SqlServer.Mssq2008SqlMapper mapper = new Mssq2008SqlMapper();

                var animalQuery = new SelectSqlBuilder(mapper, animalEntMap).WithPaging(15, 0).Build();
                var zooQuery = new SelectSqlBuilder(mapper, zooEntMap).Build();                
             
                EntitySerializerFactory serializer = new EntitySerializerFactory(mapper);
                var dataReader = dbAccess.ExecuteReader(conn, animalQuery);
                var animals = serializer.SerializeAll<WebZoo.Data.Sqlite.Animal>(dataReader, animalEntMap);
                dataReader.Dispose();

                dataReader = dbAccess.ExecuteReader(conn, zooQuery);
                var zoos = serializer.SerializeAll<WebZoo.Data.Sqlite.Zoo>(dataReader, zooEntMap);
                dataReader.Dispose();

                dataReader = dbAccess.ExecuteReader(conn, animalQuery);
                serializer.SerializeAll<WebZoo.Data.Sqlite.Animal>(dataReader, animalEntMap);
                dataReader.Dispose();

            }

            Console.WriteLine("we have sessions");
        }
    }
}

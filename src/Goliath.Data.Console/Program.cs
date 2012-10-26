﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Goliath.Data;
using Goliath.Data.Generators;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Providers.Sqlite;
using Goliath.Data.Providers.SqlServer;
using Goliath.Data.Sql;
using Goliath.Data.Transformers;
using Goliath.Data.Utils;
using System.Linq.Expressions;
using Goliath.Data.DataAccess;
using Goliath.Data.CodeGen;

namespace WebZoo.Data
{
    class Program
    {
        const string MapFileName = "GoData.Map.xml";


        static void Main(string[] args)
        {
            //var currentDir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.LastIndexOf("bin"));
            //string sqlServerWorkingDirectory = Path.Combine(currentDir, "Generated", "Mssql2008");
            //string sqliteWorkingDirectory = Path.Combine(currentDir, "Generated", "Sqlite");
            //string templatePath = currentDir;
            string template = "select @{col:a.Id}, @{sel:a.Name}, @{sel:a.City}, @{sel:a.AcceptNewAnimals} from  @{a.TableName} where @{prop:a.Id} = @{prop:b.Id}";
            string template2 = @"INSERT INTO @{TableName}(@{sel:Name},@{col:City},@{col:AcceptNewAnimals}) VALUES(@{prop:Name},@{prop:City},@{prop:AcceptNewAnimals})";

            Zoo reflZoo = new Zoo() {AcceptNewAnimals = true, City = "Refl", Name = "machin"};
            var propInfo = typeof (Zoo).GetProperty("Name");
            var getMethod = propInfo.CreateDynamicGetMethodDelegate("get_Zoo_Name");
            var nmae = getMethod(reflZoo);

            var setMethod = propInfo.CreateDynamicSetMethodDelegate("set_Zoo_Name");
            setMethod(reflZoo, "gabin");
            Console.WriteLine(reflZoo.Name);

            //si.Select<Animal>("Id", "some", "Xoe")
            //    .InnerJoin<Zoo>()
            //    .On(c => c.Id).EqualTo(c => c.ZooId)
            //    .ForJoin<Zoo>().Where(c => c.Id).EqualTo(2)
            //    .ForJoin<Employee>().And(e => e.Id).EqualTo(20);
               
           // si.SelectAll().From("users").InnerJoin("department", "dep").On("DeptID").EqualTo("u.deptId").
               
            
            Console.WriteLine("Start run");
            //Console.WriteLine(Guid.NewGuid().ToString("N"));
            MapConfig mapConfig = null;
            SupportedRdbms rdbms = SupportedRdbms.Mssql2008;
            WebZooRunner zoorunner = new WebZooRunner(rdbms, new CodeGenerator(), AppDomain.CurrentDomain.BaseDirectory, true);
            //mapConfig = zoorunner.CreateMap();
            //zoorunner.GenerateCode();


            string mapfile = Path.Combine(zoorunner.WorkingFolder, Goliath.Data.CodeGen.Constants.MapFileName);
            mapConfig = MapConfig.Create(mapfile);

            var zooEntMap = mapConfig.EntityConfigs.Where(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal))
                    .FirstOrDefault();

 
            StatementMapParser parser = new StatementMapParser();
            var compiled = parser.Parse(new SqliteDialect(), zooEntMap, template2, null);

            Dictionary<string, StatementInputParam> inputParams = new Dictionary<string, StatementInputParam> { { "a", new StatementInputParam() { Name = "a", Type = "WebZoo.Data.Zoo" } }, { "b", new StatementInputParam() { Name = "b", Type = "WebZoo.Data.Animal" } } };
            compiled = parser.Parse(new SqliteDialect(), mapConfig, inputParams, template);


            zoorunner = new WebZooRunner(SupportedRdbms.Sqlite3, new CodeGenerator(), AppDomain.CurrentDomain.BaseDirectory, true);
            mapConfig.Settings.ConnectionString = zoorunner.Settings.ConnectionString;
            QueryTest(mapConfig);
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

        static void BuildSqlite(string workingFolder)
        {
            ProjectSettings settings = new ProjectSettings();
            string pdir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin"));
            string dbfile = Path.Combine(pdir, "Data", "WebZoo.db");
            settings.ConnectionString = string.Format("Data Source={0}; Version=3", dbfile);
            settings.Namespace = "WebZoo.Data.Sqlite";
            settings.Version = "1.0";
            settings.AssemblyName = "WebZoo.Data";
            settings.BaseModel = "WebZoo.Data.BaseEntity";

            SqlDialect mapper = new SqliteDialect();
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

        static void BuildSqlServer(string workingFolder)
        {
            ProjectSettings settings = new ProjectSettings();

            settings.ConnectionString = "Data Source=localhost;Initial Catalog=DbZoo;Integrated Security=True";
            settings.Namespace = "WebZoo.Data.SqlServer";
            settings.Version = "1.0";
            settings.AssemblyName = "WebZoo.Data";
            settings.BaseModel = "WebZoo.Data.BaseEntity";
            IDbConnector dbConnector = new MssqlDbConnector(settings.ConnectionString);
            IDbAccess db = new DbAccess(dbConnector);

            SqlDialect mapper = new Mssq2008Dialect();

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

        static void QueryTest(MapConfig mapConfig)
        {
            //string sf = "/Users/hamsman/development";
            //string pdir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin"));
            //string dbfile = Path.Combine(pdir, "Data", "WebZoo.db");
            //string cs = string.Format("Data Source={0}; Version=3", dbfile);


            var sessionFactory = new Database().Configure(mapConfig)
                .RegisterProvider(new SqliteProvider()).Init();

            var sess = sessionFactory.OpenSession();

            MappedStatementRunner mapStatRunner = new MappedStatementRunner();

            int countZooStatement = mapStatRunner.RunStatement<int>(sess, "countZooStatement");

            string statName = StatementStore.BuildProcedureName(typeof(Zoo), MappedStatementType.Query);
            Console.WriteLine("Statement name {0}", statName);
            Zoo sdZoo = new Zoo() { Name = "SD Zoo", City = "San Diego", AcceptNewAnimals = true };

            MappedStatementRunner runner = new MappedStatementRunner();
            var verify = runner.RunStatement<Zoo>(sess, statName, null, sdZoo);



            var zoodapter = sess.CreateDataAccessAdapter<Zoo>();
            var animalapter = sess.CreateDataAccessAdapter<Animal>();

            var entFactory = sessionFactory.DataSerializer as Goliath.Data.DataAccess.IEntityFactory;
            var zobo = entFactory.CreateInstance<Zoo>();


            var qkw = sess.SelectAll<Animal>().Where(c => c.Name).EqualTo(c => c.Location).And(c => c.Age).GreaterOrEqualToValue(3);
            //var wlw = sess.SelectAll().From("rando").
            //ISqlInterface si = null;
            //si.SelectAll<Animal>().Where(c => c.Name).EqualTo("3").And(c => c.Id).GreaterOrEqualTo(5)
            //    .OrderBy(c => c.Name).Asc()
            //    .OrderBy(c => c.ReceivedOn).Desc().FetchAll();

            Zoo zooM = new Zoo() { Name = "Kitona", City = "Kitona", AcceptNewAnimals = true };
            var an1 = new Animal()
            {
                Age = 12,
                Location = "ERT 90-9",
                ReceivedOn = DateTime.Now,
                Name = "Lion",
                Zoo = zooM,
                //Zoo = acceptingZoos[0],
                //ZooId = acceptingZoos[0].Id,
            };

            var monkey = new Monkey()
            {
                CanDoTricks = true,
                Family = "something",
                Age = 2,
                Location = "dm kow",
                ReceivedOn = DateTime.Now,
                Name = "El Mono",
                Zoo = zooM,
            };

            var an3 = new Animal()
            {
                Age = 1,
                Location = "dw 12",
                ReceivedOn = DateTime.Now.Subtract(new TimeSpan(16, 5, 5, 5, 10)),
                Name = "donkey",
                Zoo = zooM,
                //Zoo = acceptingZoos[0],
                //ZooId = acceptingZoos[0].Id,
            };

            Employee emp1 = new Employee()
            {
                FirstName = "John",
                LastName = "Smith",
                HiredOn = DateTime.Now,
                Title = "Trainer",
                EmailAddress = "emp1@mail.com",
                AssignedToZoo = zooM
            };

            Employee emp2 = new Employee()
            {
                FirstName = "Jerry",
                LastName = "Seinfeld",
                HiredOn = DateTime.Now,
                Title = "Comedian",
                EmailAddress = "emp2@mail.com",
                AssignedToZoo = zooM
            };

            zooM.AnimalsOnZooId = new List<Animal>();
            zooM.EmployeesOnAssignedToZooId = new List<Employee>();

            zooM.AnimalsOnZooId.Add(monkey);
            zooM.AnimalsOnZooId.Add(an1);
            zooM.AnimalsOnZooId.Add(an3);

            zooM.EmployeesOnAssignedToZooId.Add(emp1);
            zooM.EmployeesOnAssignedToZooId.Add(emp2);

            emp1.AnimalsOnAnimalsHandler_EmployeeId = new List<Animal>();
            an1.EmployeesOnAnimalsHandler_AnimalId = new List<Employee>();

            emp1.AnimalsOnAnimalsHandler_EmployeeId.Add(an1);
            an1.EmployeesOnAnimalsHandler_AnimalId.Add(emp1);

            zoodapter.Insert(zooM, true);
            try
            {

                //animalapter.Insert(an1);

                //animalapter.Insert(an1, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var allzoos = zoodapter.FindAll();
            var acceptingZoos = zoodapter.FindAll(new PropertyQueryParam("AcceptNewAnimals", true));
            long total;
            var top5Zoo = zoodapter.FindAll(5, 0, out total);

            MapConfig map = mapConfig;

            //var dbConnector = new Providers.SqlServer.MssqlDbConnector("Data Source=localhost;Initial Catalog=DbZoo;Integrated Security=True");
            var dbConnector = new Goliath.Data.Providers.Sqlite.SqliteDbConnector(mapConfig.Settings.ConnectionString);
            var dbAccess = new DbAccess(dbConnector);

            using (var conn = dbConnector.CreateNewConnection())
            {
                conn.Open();
                //                string zooQuery = @"select zoo.Id as zoo_Id, zoo.Name as zoo_Name, zoo.City as zoo_City, zoo.AcceptNewAnimals as zoo_AcceptNewAnimals 
                //                from zoos zoo";

                //                string sqlQuery = @"select ani1.ZooId as ani1_ZooId, ani1.Id as ani1_Id, ani1.Name as ani1_Name, ani1.Age as ani1_Age, ani1.Location as ani1_Location, ani1.ReceivedOn as ani1_ReceivedOn  from animals ani1";
                SqliteDialect mapper = new SqliteDialect();

                var animalEntMap = map.EntityConfigs.Where(c => string.Equals(c.Name, "Animal", StringComparison.Ordinal))
                    .FirstOrDefault();

                var zooEntMap = map.EntityConfigs.Where(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal))
                    .FirstOrDefault();

                var employeeMap = map.EntityConfigs.Where(c => string.Equals(c.Name, "Employee", StringComparison.Ordinal))
                    .FirstOrDefault();

                var wst = new WhereStatement("Id");

                SelectSqlBuilder selectBuilder = new SelectSqlBuilder(mapper, animalEntMap)
                .Where(new WhereStatement("Name") { PostOperator = SqlOperator.OR }.Equals("@Name"))
                .Where(wst.NotNull());
                string sstring = selectBuilder.ToSqlString();

                UpdateSqlBuilder updateBuilder = new UpdateSqlBuilder(mapper, animalEntMap);
                var wheres = UpdateSqlBuilder.BuildWhereStatementFromPrimaryKey(animalEntMap, mapper, 0);
                string updateString = updateBuilder.Where(wheres).ToSqlString();

                DeleteSqlBuilder deleteBuilder = new DeleteSqlBuilder(mapper, animalEntMap);
                string deleteString = deleteBuilder.Where(wheres).ToSqlString();

                Console.WriteLine(sstring);
                Console.WriteLine(updateString);
                //dataReader.Read();
                //Providers.SqlServer.Mssq2008SqlMapper mapper = new Mssq2008SqlMapper();


                var leftColumn1 = new Relation() { ColumnName = "EmployeeId", PropertyName = "EmployeeId" };
                var leftcolumn2 = new Relation() { ColumnName = "AnimalId", PropertyName = "AnimalId" };

                var mapTableMap = UnMappedTableMap.Create("animals_handlers", leftColumn1, leftcolumn2);
                mapTableMap.TableAlias = "mX1";

                var currEntMap = UnMappedTableMap.Create(animalEntMap.TableName);

                SelectSqlBuilder manytomanySql = new SelectSqlBuilder(mapper, mapTableMap)
                       .AddJoin(new SqlJoin(mapTableMap, JoinType.Inner).OnTable(employeeMap).OnLeftColumn(leftColumn1).OnRightColumn("Id"))
                       .AddJoin(new SqlJoin(mapTableMap, JoinType.Inner).OnTable(currEntMap).OnLeftColumn(leftcolumn2).OnRightColumn("Id"));

                string sql = manytomanySql.ToSqlString();

                var animalQuery = new SelectSqlBuilder(mapper, animalEntMap).WithPaging(15, 0).ToSqlString();
                var zooQuery = new SelectSqlBuilder(mapper, zooEntMap).ToSqlString();

                var serializer = sess.SessionFactory.DataSerializer;

                var dataReader = dbAccess.ExecuteReader(conn, animalQuery);
                var animals = serializer.SerializeAll<Animal>(dataReader, animalEntMap);
                dataReader.Dispose();

                dataReader = dbAccess.ExecuteReader(conn, zooQuery);
                var zoos = serializer.SerializeAll<WebZoo.Data.Zoo>(dataReader, zooEntMap);
                dataReader.Dispose();

                dataReader = dbAccess.ExecuteReader(conn, animalQuery);
                serializer.SerializeAll<WebZoo.Data.Animal>(dataReader, animalEntMap);
                dataReader.Dispose();

                var m1 = animals[1];
                m1.Name = "Just_Updated";
                m1.Location = "UP345";

                Console.WriteLine(m1.EmployeesOnAnimalsHandler_AnimalId.Count);
                var aniAdapter = sess.CreateDataAccessAdapter<Animal>();
                aniAdapter.Update(m1);

            }

            Console.WriteLine("we have sessions");
            Console.ReadLine();
        }
    }
}

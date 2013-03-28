using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Goliath.Data;
using Goliath.Data.CodeGen;
using Goliath.Data.CodeGenerator;
using Goliath.Data.DataAccess;
using Goliath.Data.Mapping;
using Goliath.Data.Providers.Sqlite;
using Goliath.Data.Sql;
using Goliath.Data.Utils;

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

            var t = typeof (decimal);
            var t2 = typeof (DateTime?);
            var t3 = typeof (string);
            var t4 = typeof (DateTime);

            Console.WriteLine(t2.IsGoliathValueType());
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
            WebZooRunner zoorunner = new WebZooRunner(rdbms, new GenericCodeGenerator(), AppDomain.CurrentDomain.BaseDirectory, true);
            //mapConfig = zoorunner.CreateMap();
            //zoorunner.GenerateClasses();


            string mapfile = Path.Combine(zoorunner.WorkingFolder, Goliath.Data.CodeGen.Constants.MapFileName);
            mapConfig = MapConfig.Create(mapfile);

            var zooEntMap = mapConfig.EntityConfigs.FirstOrDefault(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal));


            StatementMapParser parser = new StatementMapParser();
            var compiled = parser.Parse(new SqliteDialect(), zooEntMap, template2, null);

            Dictionary<string, StatementInputParam> inputParams = new Dictionary<string, StatementInputParam> { { "a", new StatementInputParam() { Name = "a", Type = "WebZoo.Data.Zoo" } }, { "b", new StatementInputParam() { Name = "b", Type = "WebZoo.Data.Animal" } } };
            compiled = parser.Parse(new SqliteDialect(), mapConfig, inputParams, template);


            zoorunner = new WebZooRunner(SupportedRdbms.Sqlite3, new GenericCodeGenerator(), AppDomain.CurrentDomain.BaseDirectory, true);
            mapConfig.Settings.ConnectionString = zoorunner.Settings.ConnectionString;
            QueryTest(mapConfig);
            Console.WriteLine("done");
            //Console.ReadKey();

        }

        static void QueryTest(MapConfig mapConfig)
        {
            //string sf = "/Users/hamsman/development";
            //string pdir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin"));
            //string dbfile = Path.Combine(pdir, "Data", "WebZoo.db");
            //string cs = string.Format("Data Source={0}; Version=3", dbfile);

            MapConfig map = mapConfig;

            var sessionFactory = new Database().Configure(mapConfig)
                .RegisterProvider(new SqliteProvider()).Init();

            var sess = sessionFactory.OpenSession();

            MappedStatementRunner mapStatRunner = new MappedStatementRunner();

            int countZooStatement = mapStatRunner.RunStatement<int>(sess, "countZooStatement");

            string statName = StatementStore.BuildMappedStatementName(typeof(Zoo), MappedStatementType.Query);
            Console.WriteLine("Statement name {0}", statName);
            Zoo sdZoo = new Zoo() { Name = "SD Zoo", City = "San Diego", AcceptNewAnimals = true };

            MappedStatementRunner runner = new MappedStatementRunner();
            var verify = runner.RunStatement<Zoo>(sess, statName, null, sdZoo);

            Employee empl = new Employee { AssignedToZooId = 1, EmailAddress = "mail@com", FirstName = "Joe", HiredOn = DateTime.Now, LastName = "Smith", Title = "some edm", Telephone = "125699555" };
            empl.AssignedToZoo = new Zoo() {Name = "Kjowieoi", AcceptNewAnimals = false, City = "kilimanjaro"};
            Monkey mnk1 = new Monkey()
                                {
                                    Age = 12,
                                    CanDoTricks = true,
                                    Family = "Singe",
                                    Location = "somecage",
                                    Name = "joemonkey",
                                    ReceivedOn = DateTime.Now,
                                    ZooId = 1
                                };
            var employeeMap = map.EntityConfigs.FirstOrDefault(c => string.Equals(c.Name, "Employee", StringComparison.Ordinal));
            var animalEntMap = map.EntityConfigs.FirstOrDefault(c => string.Equals(c.Name, "Animal", StringComparison.Ordinal));
            var zooEntMap = map.EntityConfigs.FirstOrDefault(c => string.Equals(c.Name, "Zoo", StringComparison.Ordinal));
            var monkeyMap = map.EntityConfigs.FirstOrDefault(c => string.Equals(c.Name, "Monkey", StringComparison.Ordinal));

            InsertSqlBuilder insertBuild = new InsertSqlBuilder();
            var builtInst = insertBuild.Build(mnk1, monkeyMap, sess);

            foreach (var insertSqlInfo in builtInst.Statements)
            {
                var sql = insertSqlInfo.ToString(sess.SessionFactory.DbSettings.SqlDialect);
                Console.WriteLine(sql);
            }

            //var insertsqlValue = builtInst.ToString(sess.SessionFactory.DbSettings.SqlDialect);
            var relInsBuild = insertBuild.Build(empl, employeeMap, sess);
            foreach (var insertSqlInfo in relInsBuild.Statements)
            {
                var sql = insertSqlInfo.ToString(sess.SessionFactory.DbSettings.SqlDialect);
                Console.WriteLine(sql);
            }

            var zoodapter = sess.GetEntityDataAdapter<Zoo>();
            var animalapter = sess.GetEntityDataAdapter<Animal>();

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

            zoodapter.Insert(zooM);
            try
            {

                //animalapter.Insert(an1);

                //animalapter.Insert(an1, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var allzoos = zoodapter.FetchAll();
            var acceptingZoos = zoodapter.Select().Where(c => c.AcceptNewAnimals).EqualToValue(true); //zoodapter.FindAll(new PropertyQueryParam("AcceptNewAnimals", true));
            long total;
            var top5Zoo = zoodapter.FetchAll(5, 0, out total);



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
                var aniAdapter = sess.GetEntityDataAdapter<Animal>();
                aniAdapter.Update(m1);

            }

            Console.WriteLine("we have sessions");
            Console.ReadLine();
        }
    }
}

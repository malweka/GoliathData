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

            //QueryTest(sqlServerWorkingDirectory);
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
            string dbfile = Path.Combine(@"C:\Junk\Goliath.Data", "WebZoo.db");
            settings.ConnectionString = string.Format("Data Source={0}; Version=3", dbfile);
            settings.Namespace = "WebZoo.Data.Sqlite";
            settings.Version = "1.0";
            settings.AssemblyName = "WebZoo.Data";
            settings.BaseModel = "WebZoo.Data.BaseEntity";

            SqlMapper mapper = new SqliteSqlMapper();
            IDbAccess db = new SqliteDataAccess(settings.ConnectionString);
            db.ConnectionString = settings.ConnectionString;

            using (ISchemaDescriptor schema = new SqliteSchemaDescriptor(db, mapper, settings))
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
            IDbAccess db = new MssqlDataAccess(settings.ConnectionString);

            SqlMapper mapper = new Mssq2008SqlMapper();

            using (ISchemaDescriptor schemaDescriptor = new MssqlSchemaDescriptor(db, mapper, settings))
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
            string mapfile = Path.Combine(workingFolder, MapFileName);
            var sessionFactory = new Database().Configure(mapfile)
                .Provider(new MssqlProvider()).Init();

            var sess = sessionFactory.OpenSession();

            Console.WriteLine("we have sessions");
        }
    }
}

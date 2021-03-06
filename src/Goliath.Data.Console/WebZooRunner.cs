﻿using System;
using System.IO;
using Goliath.Data.CodeGenerator;
using Goliath.Data.Providers;

namespace Goliath.Data.CodeGen
{
    using Mapping;
    using Providers.Sqlite;
    using Providers.SqlServer;

    public class WebZooRunner
    {
        SupportedRdbms rdbms;
        string projectFolder;
        IGenerator codeGen;
        string scriptFolder;
        string workingFolder;
        string templateFolder;
        public ProjectSettings Settings{get; private set;}

        bool autoIncrement;

        public string ScriptFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(scriptFolder))
                {
                    string fname = "WebZooGuid";
                    if (autoIncrement)
                        fname = "WebZooAutoIncrement";
                    scriptFolder = Path.Combine(projectFolder, "Scripts", rdbms.ToString(), fname);
                }

                return scriptFolder;
            }
        }

        public string TemplateFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(templateFolder))
                {
                    templateFolder = Path.Combine(projectFolder, "Templates");
                }
                return templateFolder;
            }
        }

        public string WorkingFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(workingFolder))
                {
                    workingFolder = Path.Combine(projectFolder, "Generated");
                }
                return workingFolder;
            }
        }
		
		string databaseFolder = string.Empty;
		public string DatabaseFolder
		{
			get{ return databaseFolder;}
		}

        public WebZooRunner(SupportedRdbms rdbms, IGenerator codeGen, string databaseFolder, bool autoIncrement)
        {
            this.rdbms = rdbms;
            this.projectFolder = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin"));
            this.codeGen = codeGen;
            this.autoIncrement = autoIncrement;
			this.databaseFolder = databaseFolder;
			
            Settings = new ProjectSettings();

            if (rdbms == SupportedRdbms.Sqlite3)
            {
                string dbfile = Path.Combine(databaseFolder, "WebZoo.db");
                Settings.ConnectionString = string.Format("Data Source={0}; Version=3", dbfile);

                if (File.Exists(dbfile))
                {
                    File.Delete(dbfile);
                }

                CreateSqliteDatabase(dbfile);
            }
            else
            {
                Settings.ConnectionString = "Data Source=localhost;Initial Catalog=DbZoo;Integrated Security=True";
                CreateSqlServerDatabase();
            }


            Settings.Namespace = "WebZoo.Data";
            Settings.Version = "1.0";
            Settings.AssemblyName = "WebZoo.Data";

        }

        void CreateSqlServerDatabase()
        {
            IDbConnector dbConnector = new MssqlDbConnector(Settings.ConnectionString);
            IDbAccess db = new DbAccess(dbConnector);

            var scriptFiles = Directory.GetFiles(ScriptFolder, "*.sql", SearchOption.TopDirectoryOnly);
            using (var conn = dbConnector.CreateNewConnection())
            {
                conn.Open();
                var transaction = new TransactionWrapper(conn.BeginTransaction());
                try
                {
                    foreach (var file in scriptFiles)
                    {
                        System.Console.WriteLine("running script {0}", file);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            using (StreamReader freader = new StreamReader(fs))
                            {
                                var sql = freader.ReadToEnd();
                                db.ExecuteNonQuery(conn, transaction, sql);
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch //(Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Dispose();
            }
        }

        void CreateSqliteDatabase(string dbfile)
        {
            //SqlMapper mapper = new SqliteSqlMapper();
            IDbConnector dbConnector = new SqliteDbConnector(Settings.ConnectionString);
            IDbAccess db = new DbAccess(dbConnector);

            var scriptFiles = Directory.GetFiles(ScriptFolder, "*.sql", SearchOption.TopDirectoryOnly);
            using (var conn = dbConnector.CreateNewConnection())
            {
                conn.Open();
                var transaction = new TransactionWrapper(conn.BeginTransaction());
                try
                {
                    foreach (var file in scriptFiles)
                    {
                        System.Console.WriteLine("running script {0}", file);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            using (StreamReader freader = new StreamReader(fs))
                            {
                                var sql = freader.ReadToEnd();
                                db.ExecuteNonQuery(conn, transaction, sql);
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch //(Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Dispose();
            }
        }

        public Mapping.MapConfig CreateMap()
        {
            ComplexType baseModel = null;
            if (autoIncrement)
            {
                baseModel = new ComplexType("WebZoo.Data.BaseEntityInt");
                baseModel.Properties.Add(new Property("Id", "Id", System.Data.DbType.Int32)
                {
                    ClrType = typeof(int),
                    IsPrimaryKey = true,
                    IsUnique = true,
                });
            }
            else
            {
                baseModel = new ComplexType("WebZoo.Data.BaseEntity");
                baseModel.Properties.Add(new Property("Id", "Id", System.Data.DbType.Guid)
                {
                    ClrType = typeof(Guid),
                    IsPrimaryKey = true,
                    IsUnique = true,
                });
            }

            Settings.BaseModel = baseModel.FullName;
            SqlDialect dialect = new Mssq2008Dialect();
            IDbConnector dbConnector = new MssqlDbConnector(Settings.ConnectionString);
            IDbAccess db = new DbAccess(dbConnector);
            using (ISchemaDescriptor schemaDescriptor = new MssqlSchemaDescriptor(db, dbConnector, dialect, Settings))
            {
                return codeGen.GenerateMapping(WorkingFolder, schemaDescriptor, Settings, baseModel, rdbms, Constants.MapFileName);
            }
        }

        public void GenerateClasses()
        {
            var mapfile = Path.Combine(WorkingFolder, Constants.MapFileName);
            var templateFile = Path.Combine(TemplateFolder, "Class.razt");
            codeGen.GenerateCodeForEachEntityMap(templateFile, WorkingFolder, mapfile, (name,iteration) => name + ".cs");
        }

        public void GenerateClasses(string mapFile)
        {
            var templateFile = Path.Combine(TemplateFolder, "Class.razt");
            codeGen.GenerateCodeForEachEntityMap(templateFile, WorkingFolder, mapFile, (name,iteration) => name + ".cs");
        }

        public void GenerateClasses(MapConfig config)
        {
            var templateFile = Path.Combine(TemplateFolder, "Class.razt");
            codeGen.GenerateCodeForEachEntityMap(templateFile, WorkingFolder, config, (name,iteration) => name + ".cs");
        }
    }

    public class Constants
    {
        public const string MapFileName = "Generated_GoData.Map.xml";
    }
}

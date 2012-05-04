using System;
using System.IO;

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
            this.projectFolder = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin")); ;
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
            return codeGen.GenerateMapping(WorkingFolder, Settings, baseModel, rdbms);
        }

        public void GenerateCode()
        {
            codeGen.GenerateCode(TemplateFolder, WorkingFolder);
        }
    }

    /// <summary>
    /// Supported RDBMS
    /// </summary>
    [Flags]
    public enum SupportedRdbms
    {
        None = 0,
        /// <summary>
        /// Microsoft SQL Server 2005
        /// </summary>
        Mssql2005 = 1,
        /// <summary>
        /// Microsoft SQL Server 2008
        /// </summary>
        Mssql2008 = 2,
        /// <summary>
        /// Microsoft SQL Server 2008 R2
        /// </summary>
        Mssql2008R2 = 4,
        /// <summary>
        /// All supported version of SQL Server from 2005 to 2008 R2
        /// </summary>
        MssqlAll = Mssql2005 | Mssql2008 | Mssql2008R2,
        /// <summary>
        /// Sqlite 3
        /// </summary>
        Sqlite3 = 8,

        Postgresql8 = 16,
        /// <summary>
        /// PostgreSQL 9.x
        /// </summary>
        Postgresql9 = 32,

        /// <summary>
        /// MySql 5
        /// </summary>
        MySql5 = 64,
        /// <summary>
        /// All systems
        /// </summary>
        All = MssqlAll | Sqlite3 | Postgresql9 | MySql5,
    }
}

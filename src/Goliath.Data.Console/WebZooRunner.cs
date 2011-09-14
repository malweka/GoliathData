using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Goliath.Data.CodeGen
{
    using Generators;
    using Mapping;
    using Providers;
    using Providers.Sqlite;
    using Providers.SqlServer;
    using Sql;
    using Transformers;

    public class WebZooRunner
    {
        SupportedRdbms rdbms;
        string projectFolder;
        IGenerator codeGen;
        string scriptFolder;
        string workingFolder;
        string templateFolder;
        ProjectSettings settings;
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

        public WebZooRunner(SupportedRdbms rdbms, IGenerator codeGen, bool autoIncrement)
        {
            this.rdbms = rdbms;
            this.projectFolder = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin")); ;
            this.codeGen = codeGen;
            this.autoIncrement = autoIncrement;

            settings = new ProjectSettings();

            if (rdbms == SupportedRdbms.Sqlite3)
            {
                string dbfile = Path.Combine(WorkingFolder, "WebZoo.db");
                settings.ConnectionString = string.Format("Data Source={0}; Version=3", dbfile);

                if (!File.Exists(dbfile))
                {
                    CreateSqliteDatabase(dbfile);
                }

            }
            else
            {
                settings.ConnectionString = "Data Source=localhost;Initial Catalog=DbZoo;Integrated Security=True";
            }


            settings.Namespace = "WebZoo.Data";
            settings.Version = "1.0";
            settings.AssemblyName = "WebZoo.Data";

        }

        void CreateSqliteDatabase(string dbfile)
        {
            SqlMapper mapper = new SqliteSqlMapper();
            IDbConnector dbConnector = new SqliteDbConnector(settings.ConnectionString);
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
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Dispose();
            }
        }

        public void CreateMap()
        {
            ComplexType baseModel = null;
            if (autoIncrement)
            {
                baseModel = new ComplexType("WebZoo.Data.BaseEntityInt");
                baseModel.Properties.Add(new Property("Id", "Id", System.Data.DbType.Guid)
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

            settings.BaseModel = baseModel.FullName;
            codeGen.GenerateMapping(WorkingFolder, settings, baseModel, rdbms);
        }

        public void GenerateCode()
        {
            codeGen.GenerateCode(TemplateFolder, WorkingFolder);
        }
    }
}

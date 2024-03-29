﻿using System;
using System.IO;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.Sqlite
{
    public class SqliteDataService : ISqliteDataService
    {
        public static string BuildSqliteConnectionString(string datafile)
        {
            return $"Data Source={datafile}; Version=3";
        }

        /// <summary>
        /// Creates the database if it doesn't exist. If database file exists it will return null.
        /// </summary>
        /// <param name="databaseFilePath">The database file path.</param>
        /// <param name="ddlScripts">The DDL scripts.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">ddlScripts</exception>
        public IDbAccess CreateDatabase(string databaseFilePath, string ddlScripts)
        {
            if (File.Exists(databaseFilePath))
                return null; //File or database already exist we don't need to recreate it. Delete it before proceeding.

            if (string.IsNullOrWhiteSpace(ddlScripts))
                throw new ArgumentNullException(nameof(ddlScripts));

            string connectionString = BuildSqliteConnectionString(databaseFilePath);
            IDbConnector dbConnector = new SqliteDbConnector(connectionString);
            IDbAccess db = new DbAccess(dbConnector);
            //string ddl = Utilities.InternalResources.DDL.TrimEnd();

            using (var conn = dbConnector.CreateNewConnection())
            {
                conn.Open();
                db.ExecuteNonQuery(conn, ddlScripts);
            }
            return db;
        }

        /// <summary>
        /// Creates the database if it doesn't exist. If database file exists it will return null.
        /// </summary>
        /// <param name="databaseFilePath">The database file.</param>
        /// <param name="ddlScriptStream"> </param>
        /// <returns></returns>
        public IDbAccess CreateDatabase(string databaseFilePath, Stream ddlScriptStream)
        {
            if (ddlScriptStream == null) throw new ArgumentNullException(nameof(ddlScriptStream));

            var ddlScript = ddlScriptStream.ConvertToString();
            return CreateDatabase(databaseFilePath, ddlScript);
        }

       

        /// <summary>
        /// Creates the session factory.
        /// </summary>
        /// <param name="mapfile">The mapfile.</param>
        /// <param name="databaseFile">The database file.</param>
        /// <returns></returns>
        public ISessionFactory CreateSessionFactory(string mapfile, string databaseFile)
        {
            var projSettings = new ProjectSettings
                                   {
                                       Platform = RdbmsBackend.SupportedSystemNames.Sqlite3,
                                       Namespace = "MyTerritory.Data", //TODO: remove this hardcoded namespace
                                       ConnectionString = BuildSqliteConnectionString(databaseFile)
                                   };
            var sessionFactory = new Database().Configure(mapfile, projSettings).RegisterProvider(new SqliteProvider()).Init();
            return sessionFactory;
        }
    }
}

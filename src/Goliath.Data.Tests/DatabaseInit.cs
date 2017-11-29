using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Common;

namespace Goliath.Data.Tests
{
    using DataAccess;
    using Mapping;
    using Providers;
    using Providers.Sqlite;

    /// <summary>
    /// 
    /// </summary>
    public static class DatabaseInit
    {
        /// <summary>
        /// Creates the database.
        /// </summary>
        /// <param name="mapFolder">The map folder.</param>
        /// <param name="dbFileName">Name of the db file.</param>
        public static void CreateSqliteDatabase(MapConfig config, string scriptFolder, string dbFileName)
        {
            //string autoIncrementFileName = string.Format("{0}_auto_increment.{1}.db", dbFileName, DateTime.Now.Ticks);
            //string guidFileName = string.Format("{0}_guid.{1}.db", dbFileName, DateTime.Now.Ticks);

            if (File.Exists(dbFileName))
            {
                File.Delete(dbFileName);
            }

            IDbConnector dbConnector = new SqliteDbConnector(config.Settings.ConnectionString);
            IDbAccess db = new DbAccess(dbConnector);

            var scriptFiles = Directory.GetFiles(scriptFolder, "*.sql", SearchOption.TopDirectoryOnly);

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
    }

    class TransactionWrapper : ITransaction
    {
        DbTransaction transaction;

        public TransactionWrapper(DbTransaction transaction)
        {
            this.transaction = transaction;
        }
        #region ITransaction Members

        public DbTransaction InnerTransaction => transaction;

        public bool IsStarted { get; private set; }
        public bool WasCommitted { get; private set; }
        public bool WasRolledBack { get; private set; }

        public void Begin()
        {

        }

        public void Begin(System.Data.IsolationLevel isolatedLevel)
        {
            //throw new NotImplementedException();
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public void Rollback()
        {
            transaction.Rollback();
        }

        public void Enlist(System.Data.IDbCommand command)
        {
            command.Transaction = transaction;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            transaction.Dispose();
        }

        #endregion
    }
}

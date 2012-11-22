using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Goliath.Data.DataAccess;
using Goliath.Data.Sql;
using Goliath.Data.Mapping;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class SqlCommandRunnerTests
    {
        ISession session;


        [TestFixtureSetUp]
        public void Initialize()
        {
            session = SessionHelper.Factory.OpenSession();
        }

        [Test]
        public void ExecuteReader_Read_dynamically_and_unmapped_entity()
        {
            string tableName = "tst_SCR_01";
            string createTableFormat = "create table {0}(Id integer primary key autoincrement not null, Prop1 nvarchar(50) not null, Prop2 datetime);";
            string insertStatementFormat = "Insert into {0}(Prop1, Prop2) Values($prop1, $prop2);";

            var transaction = session.BeginTransaction();
            var dbConn = session.ConnectionManager.OpenConnection();
            session.DataAccess.ExecuteNonQuery(dbConn, transaction, string.Format(createTableFormat, tableName));

            for (int i = 1; i < 6; i++)
            {
                session.DataAccess.ExecuteNonQuery(dbConn, transaction, string.Format(insertStatementFormat, tableName), new QueryParam("prop1") { Value = string.Format("val{0}", i) }, new QueryParam("prop2") { Value = DateTime.Now.AddDays(i) });
            }

            session.CommitTransaction();
            string query = "select Id, Prop1, Prop2 from " + tableName + " where Prop1 = 'val2'";
            DynamicEntityMap dmap = new DynamicEntityMap(null, tableName, typeof(SqlCommandRunnerTestsFake1));
            SqlCommandRunner runner = new SqlCommandRunner();

            SqlCommandRunnerTestsFake1 ent1 = runner.ExecuteReader<SqlCommandRunnerTestsFake1>(dmap, dbConn, session, query).FirstOrDefault();
            Assert.AreEqual("val2", ent1.Prop1);

        }
    }

    public class SqlCommandRunnerTestsFake1
    {
        public int Id { get; set; }
        public string Prop1 { get; set; }
        public DateTime Prop2 { get; set; }
    }
}

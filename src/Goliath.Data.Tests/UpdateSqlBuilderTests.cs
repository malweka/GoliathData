using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Sql;
using NUnit.Framework;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class UpdateSqlBuilderTests
    {
        ISession session;

        [TestFixtureSetUp]
        public void Initialize()
        {
            session = SessionHelper.Factory.OpenSession();
        }

        [Test]
        public void Build_statement_with_only_one_where_statement()
        {
            string verify = "INSERT INTO faketable(col1, col2, col3) VALUES ($col1, $col2, $col3) WHERE col1 = $qPm0";
            var columns = new List<string>() {"col1", "col2", "col3"};
            var parameters = new List<QueryParam>
                                 {
                                     new QueryParam("col1", null),
                                     new QueryParam("col2", null),
                                     new QueryParam("col3", null)
                                 };
            var builder = new UpdateSqlBuilder("faketable", session, columns, parameters);
            builder.Where("col1")
                .EqualToValue(1);

            var sqlBody = builder.Build();
            var res = sqlBody.ToString(session.SessionFactory.DbSettings.SqlDialect);
            Console.WriteLine(res);
            Assert.AreEqual(verify, res);
        }

        [Test, ExpectedException(typeof(DataAccessException))]
        public void Build_statement_with_no_where_should_throw()
        {
            var columns = new List<string>() { "col1", "col2", "col3" };
            var parameters = new List<QueryParam>
                                 {
                                     new QueryParam("col1", null),
                                     new QueryParam("col2", null),
                                     new QueryParam("col3", null)
                                 };
            var builder = new UpdateSqlBuilder("faketable", session, columns, parameters);
            builder.Build();
            Assert.Fail("there was no where specified, should have thrown");
        }

        [Test]
        public void Build_update_statment_with_several_where()
        {
            string verify = "INSERT INTO faketable(col1, col2, col3) VALUES ($col1, $col2, $col3) WHERE col1 = $qPm0 AND col2 >= $qPm1 OR col3 = col1";
            var columns = new List<string>() { "col1", "col2", "col3" };
            var parameters = new List<QueryParam>
                                 {
                                     new QueryParam("col1", null),
                                     new QueryParam("col2", null),
                                     new QueryParam("col3", null)
                                 };
            var builder = new UpdateSqlBuilder("faketable", session, columns, parameters);

            builder.Where("col1").EqualToValue(1)
                .And("col2").GreaterOrEqualToValue(10)
                .Or("col3").EqualTo("col1");

            var sqlBody = builder.Build();
            var res = sqlBody.ToString(session.SessionFactory.DbSettings.SqlDialect);
            Console.WriteLine(res);
            Assert.AreEqual(verify, res);
        }
    }
}

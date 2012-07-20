using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Goliath.Data.DataAccess;
using Goliath.Data.Sql;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class QueryBuilderTests
    {
        ISession session;

        [TestFixtureSetUp]
        public void Initialize()
        {
            session = SessionHelper.Factory.OpenSession();
        }

        QueryBuilder CreateBuilder(params string[] columns)
        {
            var dialect = session.SessionFactory.DbSettings.SqlDialect;
            var mapping = session.SessionFactory.DbSettings.Map;

            return new QueryBuilder(dialect, columns.ToList(), mapping);
        }

        [Test]
        public void BuildSql_build_valid_select_with_joins()
        {
            string statement = "SELECT Id, Name, Age FROM tb_fakeTable ftb INNER JOIN tb_join1 j1 ON ftb.jid = j1.jcol1 LEFT JOIN tb_join2 j2 ON ftb.j2id = j2.jcol2";
            var query = session
                .Select("Id", "Name", "Age")
                .From("tb_fakeTable", "ftb");

                query.InnerJoin("tb_join1", "j1")
                    .On("jcol1").EqualTo("jid")
                .LeftJoin("tb_join2", "j2")
                    .On("jcol2").EqualTo("j2id");

            QueryBuilder builder = query as QueryBuilder;
            string sql = builder.BuildSql();
            Console.WriteLine(sql);
            Assert.AreEqual(statement, sql.Trim());
        }

        [Test]
        public void BuildSql_with_where_should_compose_sql_with_where_statement()
        {
            string stat = "SELECT id, name, age, location FROM tb_fakeTable fkt WHERE fkt.name LIKE $qPm0 AND fkt.age > $qPm1 OR fkt.location = $qPm2";

            var query = session.Select("id", "name", "age", "location").From("tb_fakeTable", "fkt");

            query.Where("name").LikeValue("jam%")
                .And("age").GreaterThanValue(45)
                .Or("location").EqualToValue("NoWhere");

            QueryBuilder builder = query as QueryBuilder;
            string sql = builder.BuildSql();
            Console.WriteLine(sql);

            Assert.AreEqual(3, builder.Parameters.Count);
            Assert.AreEqual(stat, sql.Trim());
            Assert.AreEqual(builder.Parameters[1].Value, 45);
            Assert.AreEqual(builder.Parameters[2].Value, "NoWhere");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Sql;
using NUnit.Framework;
using WebZoo.Data;
using Goliath.Data.Utils;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class UpdateSqlBuilderFluentTests
    {
        ISession session;

        [TestFixtureSetUp]
        public void Initialize()
        {
            session = SessionHelper.Factory.OpenSession();
        }


        [Test]
        public void Load_should_load_all_properties_even_parent_class()
        {
            Monkey monkey = new Monkey()
                                {
                                    Id = 1,
                                    Name = "shaker",
                                    Age = 2,
                                    Family = "unknown",
                                    CanDoTricks = true,
                                    Location = "XER3",
                                    ReceivedOn = DateTime.Now
                                };

            UpdateSqlBuilder<Monkey> builder = new UpdateSqlBuilder<Monkey>(session, monkey);
            Assert.AreEqual(2, builder.Build().Statements.Count);
        }
    }
}

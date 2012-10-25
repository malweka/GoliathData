using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Goliath.Data.DataAccess;
using System.Data;
using System.Data.Common;
using Goliath.Data.Providers;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class UserProvidedConnectionProviderTests
    {
        [Test]
        public void GetConnection_should_keep_same_connection_that_user_submitted()
        {
            var connector = SessionHelper.Factory.DbSettings.Connector;
            var connProv = new ConnectionProvider(connector);
            var conn1 = connProv.GetConnection();

            var uprov = new UserProvidedConnectionProvider(conn1);
            var conn2 = uprov.GetConnection();

            Assert.IsNotNull(conn1);
            Assert.IsNotNull(conn2);

            Assert.AreEqual(conn1, conn2);
            conn1.Dispose();
        }

        [Test]
        public void DiscardOfConnection_should_do_nothing_but_dereference()
        {
            var connector = SessionHelper.Factory.DbSettings.Connector;
            var connProv = new ConnectionProvider(connector);
            var conn1 = connProv.GetConnection();

            var uprov = new UserProvidedConnectionProvider(conn1);
            var conn2 = uprov.GetConnection();
            conn2.Open();

            uprov.DiscardOfConnection(conn2);
            Assert.IsTrue(conn2.State == ConnectionState.Open);
        }

        [Test, ExpectedException(typeof(GoliathDataException))]
        public void DiscardOfConnection_should_throw_if_trying_discard_connection_that_not_managed()
        {
            var connector = SessionHelper.Factory.DbSettings.Connector;
            var connProv = new ConnectionProvider(connector);
            var conn1 = connProv.GetConnection();

            var uprov = new UserProvidedConnectionProvider(conn1);
            var conn2 = uprov.GetConnection();

            uprov.DiscardOfConnection(connProv.GetConnection());
        }
    }
}

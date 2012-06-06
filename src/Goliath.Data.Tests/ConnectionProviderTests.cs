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
    public class ConnectionProviderTests
    {
        [Test, ExpectedException(typeof(System.ArgumentNullException))]
        public void Constructor_null_connector_should_throw()
        {
            ConnectionProvider prov = new ConnectionProvider(null);
            Assert.Fail("should have thrown a null argument exception");
        }

        [Test]
        public void GetConnection_create_new_connection_should_create_new_connection_always()
        {
            var connector = SessionHelper.Factory.DbSettings.Connector;
            var connProv = new ConnectionProvider(connector);
            var conn1 = connProv.GetConnection();
            var conn2 = connProv.GetConnection();

            Assert.IsNotNull(conn1);
            Assert.IsNotNull(conn2);
            Assert.AreNotEqual(conn1, conn2);

            conn1.Dispose();
            conn2.Dispose();
        }

        [Test]
        public void DiscardOfConnection_discard_of_opened_connection()
        {
            var connector = SessionHelper.Factory.DbSettings.Connector;
            var connProv = new ConnectionProvider(connector);
            var conn1 = connProv.GetConnection();
            conn1.Open();

            connProv.DiscardOfConnection(conn1);

            //Assert.IsTrue(conn1.State == ConnectionState.Closed);
        }
    }
}

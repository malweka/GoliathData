using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

using Goliath.Data.DataAccess;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class AdoTransactionTests
    {
        ISession session;

        [TestFixtureSetUp]
        public void Initialize()
        {
            session = SessionHelper.Factory.OpenSession();
        }

        [Test]
        public void Constructor_requires_Session_Should_Create_AdoTransaction()
        {
            using (var transaction = new AdoTransaction(session))
            {
                Assert.IsNotNull(transaction);
            }
           
        }

        [Test]
        public void Constructor_null_session_throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AdoTransaction(null));
        }

        [Test]
        public void Commit_transaction_needs_to_be_started_before_commit_throws()
        {
            AdoTransaction transaction = new AdoTransaction(session);
            transaction.Commit();
            Assert.Throws<DataAccessException>(() => transaction.Commit());
        }
    }
}

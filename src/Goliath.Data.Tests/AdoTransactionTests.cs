using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;

using Goliath.Data.DataAccess;

namespace Goliath.Data.Tests
{
    [TestFixture, TestsOn(typeof(AdoTransaction))]
    public class AdoTransactionTests
    {
        ISession session;

        [FixtureSetUp]
        void Initialize()
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

        [Test, ExpectedArgumentNullException]
        public void Constructor_null_session_throws()
        {
            AdoTransaction transaction = new AdoTransaction(null);
            Assert.Fail("should have thrown a null argument exception");
        }

        [Test, ExpectedException(typeof(DataAccessException))]
        public void Commit_transaction_needs_to_be_started_before_commit_throws()
        {
            AdoTransaction transaction = new AdoTransaction(session);
            transaction.Commit();
            Assert.Fail("should have thrown a DataAccessException");
        }
    }
}

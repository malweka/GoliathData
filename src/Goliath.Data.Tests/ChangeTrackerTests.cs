using System;
using NUnit.Framework;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class ChangeTrackerTests
    {
        [Test]
        public void Changed_boolean_TrackableShould_be_dirty()
        {
            var m = new ChangeModelTester(false);

            m.IsBool = true;
            m.ChangeTracker.Init();
            m.ChangeTracker.LoadInitialValue("IsBool", true);
            m.ChangeTracker.Start();
            m.IsBool = false;

            Assert.IsTrue(m.IsDirty);
        }

        [Test]
        public void no_Change_boolean_Trackable_Should_not_be_dirty()
        {
            var m = new ChangeModelTester(false);

            m.IsBool = true;
            m.ChangeTracker.Init();
            m.ChangeTracker.LoadInitialValue("IsBool", true);
            m.ChangeTracker.Start();
            m.IsBool = true;

            Assert.IsFalse(m.IsDirty);
        }


        [Test]
        public void Changed_null_boolean_TrackableShould_be_dirty()
        {
            var m = new ChangeModelTester(false);

            m.IsNullableBool = true;
            m.ChangeTracker.Init();
            m.ChangeTracker.LoadInitialValue("IsNullableBool", true);
            m.ChangeTracker.Start();
            m.IsNullableBool = false;

            Assert.IsTrue(m.IsDirty);
        }

        [Test]
        public void no_Change_nullable_boolean_Trackable_Should_not_be_dirty()
        {
            var m = new ChangeModelTester(false);

            m.IsNullableBool = true;
            m.ChangeTracker.Init();
            m.ChangeTracker.LoadInitialValue("IsNullableBool", true);
            m.ChangeTracker.Start();
            m.IsNullableBool = true;

            Assert.IsFalse(m.IsDirty);
        }

        [Test]
        public void Changed_date_TrackableShould_be_dirty()
        {
            var m = new ChangeModelTester(false);

            var initVal = DateTime.Now;

            m.TheDateTime = initVal;
            m.ChangeTracker.Init();
            m.ChangeTracker.LoadInitialValue("TheDateTime", initVal);
            m.ChangeTracker.Start();
            m.TheDateTime = DateTime.Now.AddMinutes(5);

            Assert.IsTrue(m.IsDirty);
        }

        [Test]
        public void no_Change__date_Trackable_Should_not_be_dirty()
        {
            var m = new ChangeModelTester(false);
            var initVal = DateTime.Now;
            m.TheDateTime = initVal;
            m.ChangeTracker.Init();
            m.ChangeTracker.LoadInitialValue("TheDateTime", initVal);
            m.ChangeTracker.Start();
            m.TheDateTime = initVal;

            Assert.IsFalse(m.IsDirty);
        }

        [Test]
        public void Changed_int_TrackableShould_be_dirty()
        {
            var m = new ChangeModelTester(false);

            var initVal = 2;

            m.Dummy = initVal;
            m.ChangeTracker.Init();
            m.ChangeTracker.LoadInitialValue("Dummy", initVal);
            m.ChangeTracker.Start();
            m.Dummy = 5;

            Assert.IsTrue(m.IsDirty);
        }

        [Test]
        public void no_Change__int_Trackable_Should_not_be_dirty()
        {
            var m = new ChangeModelTester(false);
            var initVal = 2;
            m.Dummy = initVal;
            m.ChangeTracker.Init();
            m.ChangeTracker.LoadInitialValue("Dummy", initVal);
            m.ChangeTracker.Start();
            m.Dummy = initVal;

            Assert.IsFalse(m.IsDirty);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Utils;
using NUnit.Framework;
using WebZoo.Data;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class EntityAccessorStoreTests
    {
        [Test]
        public void GetEntityAccessor_with_null_entityType_should_throw()
        {
            var store = new EntityAccessorStore();
            Assert.Throws<ArgumentNullException>(() => store.GetEntityAccessor(null));
            Assert.Fail("entityType was null. Should have thrown an exception.");
        }

        [Test]
        public void GetEntityAccessor_with_null_entityMap_should_return_EntityAccessor_which_is_not_ready()
        {
            var store = new EntityAccessorStore();
            var entityAccessor = store.GetEntityAccessor(typeof(FakeEntityAccessor1), null);

            Assert.IsFalse(entityAccessor.IsReady);
        }

        [Test]
        public void GetEntityAccessor_with_entityMap_should_return_accessor_with_loaded_properties()
        {
            var store = new EntityAccessorStore();
            var map = new Mapping.DynamicEntityMap(typeof(FakeEntityAccessor2));
            var accessor = store.GetEntityAccessor(typeof(FakeEntityAccessor2), map);

            Assert.IsTrue(accessor.IsReady);
            Assert.AreEqual(4, accessor.Properties.Count);
        }
    }

    public class FakeEntityAccessor1
    {
        public virtual string Prop { get; set; }
        public virtual string Prop1 { get; set; }
        public virtual object Prop2 { get; set; }
    }

    public class FakeEntityAccessor2
    {
        public virtual string Prop1 { get; set; }
        public virtual object Prop2 { get; set; }
        public virtual int Prop3 { get; set; }
        public virtual double Prop4 { get; set; }
    }
}

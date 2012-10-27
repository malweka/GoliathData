using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;
using NUnit.Framework;
using WebZoo.Data;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class EntityAccessorTests
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_with_null_type_should_throw()
        {
            var accessor = new EntityAccessor(null);
            Assert.Fail("Type was null, should have thrown null argument exception");
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Load_with_null_entityMap_should_throw()
        {
            var accessor = new EntityAccessor(typeof(Zoo));
            accessor.Load(null);
            Assert.Fail("entityMap was null should have thrown");
        }

        [Test]
        public void LoadedEntity_should_provide_getter_and_setter_methods()
        {
            var accessor = new EntityAccessor(typeof(FakeEntityAccessor3));
            accessor.Load(new DynamicEntityMap(typeof(FakeEntityAccessor3)));

            FakeEntityAccessor3 fake = new FakeEntityAccessor3(){ Prop = "Hello"};
            var pAccProp = accessor.Properties["Prop"];
            Assert.AreEqual("Hello", pAccProp.GetMethod(fake));

            var pAccProp1 = accessor.Properties["Prop1"];
            pAccProp1.SetMethod(fake, "try me");
            Assert.AreEqual("try me", fake.Prop1);

        }
    }

    public class FakeEntityAccessor3
    {
        public virtual string Prop { get; set; }
        public virtual string Prop1 { get; set; }
        public virtual object Prop2 { get; set; }
    }
}

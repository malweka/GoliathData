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
        [Test]
        public void Ctor_with_null_type_should_throw()
        {
            Assert.Throws<ArgumentNullException>(() => new EntityAccessor(null));
        }

        [Test]
        public void Load_with_null_entityMap_should_throw()
        {
            var accessor = new EntityAccessor(typeof(Zoo));
            Assert.Throws<ArgumentNullException>(() => accessor.Load(null));
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

        [Test]
        public void EntityProcessor_Property_Get_Method_Should_Set_and_Get_value_types()
        {
            DateTime dateValue = DateTime.Now;
            DateTime verifyDate = DateTime.Now.AddDays(2);

            var accessor = new EntityAccessor(typeof(FakeEntityAccessor3));
            accessor.Load(new DynamicEntityMap(typeof(FakeEntityAccessor3)));
            FakeEntityAccessor3 fake = new FakeEntityAccessor3() { Prop = "Hello", Prop2 = dateValue, Prop3 = 5 };

            var dateProp = accessor.Properties["Prop2"];
            Assert.AreEqual(dateValue, dateProp.GetMethod(fake));
            dateProp.SetMethod(fake, verifyDate);
            Assert.AreEqual(verifyDate, fake.Prop2);

            var intProp = accessor.Properties["Prop3"];
            Assert.AreEqual(5, intProp.GetMethod(fake));
            intProp.SetMethod(fake, 8);
            Assert.AreEqual(8, fake.Prop3);
        }

        [Test]
        public void EntityProcessor_Property_Get_Method_Should_Set_and_Get_nullable_value_types()
        {
            DateTime dateValue = DateTime.Now;
            DateTime verifyDate = DateTime.Now.AddDays(2);
            long val = 8;
            var accessor = new EntityAccessor(typeof(FakeEntityAccessor3));
            accessor.Load(new DynamicEntityMap(typeof(FakeEntityAccessor3)));
            FakeEntityAccessor3 fake = new FakeEntityAccessor3() { Prop = "Hello", Prop4 = 5 };


            var nullableProp = accessor.Properties["Prop4"];
            Assert.AreEqual(5, nullableProp.GetMethod(fake));
            nullableProp.SetMethod(fake, val);
            Assert.AreEqual(val, fake.Prop4);

            //now let's handle null values
            nullableProp.SetMethod(fake, null);
            Assert.IsNull(fake.Prop4);
            Assert.IsNull(nullableProp.GetMethod(fake));
            
        }
    }

    public class FakeEntityAccessor3
    {
        public virtual string Prop { get; set; }
        public virtual string Prop1 { get; set; }
        public virtual DateTime Prop2 { get; set; }
        public virtual int Prop3 { get; set; }
        public virtual long? Prop4 { get; set; }
    }
}

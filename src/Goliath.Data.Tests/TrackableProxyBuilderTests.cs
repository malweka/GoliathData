using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.DynamicProxy;
using Goliath.Data.Entity;
using Goliath.Data.Mapping;
using NUnit.Framework;
using Goliath.Data.Utils;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class TrackableProxyBuilderTests
    {
        [Test]
        public void Create_proxy_with_valid_entity_map_and_null_proxyhadrator_should_create_a_proxy_already_loaded()
        {
            Type fakeType = typeof (ProxyFakeClassTest);
            var entMap = new DynamicEntityMap("faket", "faketable", fakeType);
            var obj = fakeType.CreateProxy(entMap, false);
            Assert.IsNotNull(obj);
            Assert.IsNotAssignableFrom(fakeType, obj);
            var lazyObj = (ILazyObject) obj;
            Assert.IsTrue(lazyObj.IsProxyLoaded);
            Assert.AreEqual(fakeType, lazyObj.ProxyOf);
        }

        [Test]
        public void Create_proxy_with_valid_entity_map_and_null_proxyhadrator_with_implement_ITrackable_true_should_return_trackable_entity()
        {
            Type fakeType = typeof(ProxyFakeClassTest);
            var entMap = new DynamicEntityMap("faket", "faketable", fakeType);
            var obj = fakeType.CreateProxy(entMap, true);
            Assert.IsNotNull(obj);
            Assert.IsNotAssignableFrom(fakeType, obj);
            var trackable = (ITrackable)obj;
            Assert.IsFalse(trackable.IsDirty);
            //Assert.IsNotNull(trackable);
        }
    }

    public class ProxyFakeClassTest
    {
        public virtual long FakeId { get; set; }
        public virtual string FakeName { get; set; }
        public virtual DateTime FakeDate { get; set; }
    }

}

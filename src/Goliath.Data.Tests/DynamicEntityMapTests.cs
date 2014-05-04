using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Goliath.Data.DataAccess;
using Goliath.Data.Sql;
using Goliath.Data.Mapping;

namespace Goliath.Data.Tests
{
    [TestFixture]
    public class DynamicEntityMapTests
    {
        [Test]
        public void Ctor_Load_Properties_as_column()
        {
            DynamicEntityMap dmap = new DynamicEntityMap("tb", "tableFake", typeof(DynamicEntityFake));
            Assert.AreEqual(4, dmap.Properties.Count);
            Assert.AreEqual("tb", dmap.TableAlias);
            Assert.AreEqual("tableFake", dmap.TableName);
            Assert.AreEqual("Name", dmap["Name"].ColumnName);
            Assert.AreEqual("Prop3", dmap["Prop3"].ColumnName);
        }

        [Test,  ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_null_type_should_throw()
        {
            DynamicEntityMap dmap = new DynamicEntityMap("tb", "tableFake", null);
            Assert.Fail("Should have thrown, type is null");
        }
    }

    public class DynamicEntityFake
    {
        public string Name { get; set; }
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public string Prop3 { get; set; }
    }
}

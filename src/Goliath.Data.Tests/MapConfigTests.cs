﻿using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using System.IO;

namespace Goliath.Data.Tests
{
    using Mapping;

    [TestFixture]
    public class MapConfigTests
    {
        [Test]
        public void Load_load_simple_xml_config_should_have_valid_map_config()
        {
            string testMapfile = Path.Combine(SessionHelper.BaseDirectory, "TestFiles", "Test001.data.xml");
            MapConfig config = new MapConfig();
            config.Load(testMapfile);

            Assert.AreEqual(2, config.EntityConfigs.Count);
        }
    }
}

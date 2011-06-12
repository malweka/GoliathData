using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Mapping;
using Goliath.Data.Diagnostics;
using Goliath.Data.Providers;
using Goliath.Data.Config;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class Database
    {
        IConfigurationManager configManager;

        /// <summary>
        /// Configures the specified map file.
        /// </summary>
        /// <param name="mapFile">The map file.</param>
        /// <returns></returns>
        public IConfigurationManager Configure(string mapFile)
        {
            MapConfig map = MapConfig.Create(mapFile);
            return Configure(map);
        }

        /// <summary>
        /// Configures the specified map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns></returns>
        public IConfigurationManager Configure(MapConfig map)
        {
            configManager = new ConfigManager(map);
            return configManager;
        }


        //IDataAccessAdaterFactory daFactory;

        //internal IDataAccessAdaterFactory DataAccessAdapterFactory
        //{
        //    get { return daFactory; }
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        public Database()
        {
        }
    }
}

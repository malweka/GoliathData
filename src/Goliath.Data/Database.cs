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
		
		public IConfigurationManager Configure(string mapFile)
        {
            return Configure(mapFile, null);
        }

        /// <summary>
        /// Configures the specified map file.
        /// </summary>
        /// <param name="mapFile">The map file.</param>
        /// <returns></returns>
        public IConfigurationManager Configure(string mapFile, string connectionString)
        {
			if(string.IsNullOrWhiteSpace(mapFile))
				throw new ArgumentNullException("mapFile");
			
            MapConfig map = MapConfig.Create(mapFile);
			if(!string.IsNullOrWhiteSpace(connectionString))
				map.Settings.ConnectionString = connectionString;
			
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    using Mapping;
    using Diagnostics;
    using Providers;
    using Config;

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
        /// <param name="sessionStore">The session store.</param>
        /// <returns></returns>
		public IConfigurationManager Configure(string mapFile, ISessionStore sessionStore = null)
        {
            return Configure(mapFile, string.Empty, null);
        }

        /// <summary>
        /// Configures the specified map file.
        /// </summary>
        /// <param name="mapFile">The map file.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="sessionStore">The session store.</param>
        /// <returns></returns>
        public IConfigurationManager Configure(string mapFile, string connectionString, ISessionStore sessionStore = null)
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
        public IConfigurationManager Configure(MapConfig map, ISessionStore sessionStore = null)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (sessionStore == null)
            {
                sessionStore = new ThreadStaticSessionStore();
            }
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

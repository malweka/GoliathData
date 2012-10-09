using System;
using System.IO;
using Goliath.Data.Config;
using Goliath.Data.Mapping;

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
        /// <param name="sessionStore">The session store.</param>
        /// <returns></returns>
		public IConfigurationManager Configure(string mapFile, ISessionStore sessionStore = null)
        {
            return Configure(mapFile, null, null);
        }

        /// <summary>
        /// Configures the specified map file.
        /// </summary>
        /// <param name="mapFile">The map file.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="sessionStore">The session store.</param>
        /// <returns></returns>
        public IConfigurationManager Configure(string mapFile, ProjectSettings settings, ISessionStore sessionStore = null)
        {
			if(string.IsNullOrWhiteSpace(mapFile))
				throw new ArgumentNullException("mapFile");
			
            MapConfig map;
            if (settings != null)
                map = MapConfig.Create(mapFile, settings);
            else
                map = MapConfig.Create(mapFile);

            return Configure(map, sessionStore);
        }

        public IConfigurationManager Configure(Stream mapStream, ProjectSettings settings, ISessionStore sessionStore = null)
        {
            if (mapStream == null)
                throw new ArgumentNullException("mapStream");

            MapConfig map;
            if (settings != null)
                map = MapConfig.Create(mapStream, settings);
            else
                map = MapConfig.Create(mapStream);

            return Configure(map, sessionStore);
        }

        /// <summary>
        /// Configures the specified map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="sessionStore">The session store.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        public Database()
        {
        }
    }
}

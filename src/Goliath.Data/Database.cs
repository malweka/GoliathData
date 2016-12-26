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
        public IConfigurationManager Configure(string mapFile, ISessionStore sessionStore = null, params IKeyGenerator[] generators)
        {
            return Configure(mapFile, null, sessionStore);
        }

        /// <summary>
        /// Configures the specified map file.
        /// </summary>
        /// <param name="mapFile">The map file.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="sessionStore">The session store.</param>
        /// <returns></returns>
        public IConfigurationManager Configure(string mapFile, ProjectSettings settings, ISessionStore sessionStore = null, params IKeyGenerator[] generators)
        {
            if (string.IsNullOrWhiteSpace(mapFile))
                throw new ArgumentNullException(nameof(mapFile));

            MapConfig map;
            if (settings != null)
                map = MapConfig.Create(mapFile, settings, false, generators);
            else
                map = MapConfig.Create(mapFile, false, generators);

            return Configure(map, sessionStore);
        }

        /// <summary>
        /// Configures the specified map stream.
        /// </summary>
        /// <param name="mapStream">The map stream.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="sessionStore">The session store.</param>
        /// <param name="generators">The generators.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">mapStream</exception>
        public IConfigurationManager Configure(Stream mapStream, ProjectSettings settings, ISessionStore sessionStore = null, params IKeyGenerator[] generators)
        {
            if (mapStream == null)
                throw new ArgumentNullException(nameof(mapStream));

            var map = settings != null ? MapConfig.Create(mapStream, settings, false, generators) : MapConfig.Create(mapStream, false, generators);

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
                throw new ArgumentNullException(nameof(map));

            if (sessionStore == null)
            {
                sessionStore = new ThreadStaticSessionStore();
            }

            configManager = new ConfigManager(map);
            return configManager;
        }
    }
}

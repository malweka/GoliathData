using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// Entities Map and config model
    /// </summary>
    [Serializable]
    [DataContract]
    public partial class MapConfig
    {
        internal const string XmlNameSpace = "http://schemas.hamsman.com/goliath/data/1.1";
        private List<StatementMap> unprocessedStatements = new List<StatementMap>();
        static ILogger logger;

        static MapConfig()
        {
            logger = Logger.GetLogger(typeof(MapConfig));
        }

        /// <summary>
        /// Gets the unprocessed statements.
        /// </summary>
        /// <value>
        /// The unprocessed statements.
        /// </value>
        public List<StatementMap> UnprocessedStatements
        {
            get { return unprocessedStatements; }
        }

        #region Properties

        /// <summary>
        /// Gets or sets the entity maps.
        /// </summary>
        /// <value>The entity configs.</value>
        [DataMember]
        public EntityCollection EntityConfigs { get; set; }

        /// <summary>
        /// Gets or sets the complex types.
        /// </summary>
        /// <value>The complex types.</value>
        [DataMember]
        public ComplexTypeCollection ComplexTypes { get; set; }

        /// <summary>
        /// Gets or sets the mapped statements.
        /// </summary>
        /// <value>
        /// The mapped statements.
        /// </value>
        [DataMember]
        public StatementStore MappedStatements { get; set; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        [DataMember]
        public ProjectSettings Settings { get; internal set; }

        /// <summary>
        /// Gets or sets the primary key generator store.
        /// </summary>
        /// <value>
        /// The primary key generator store.
        /// </value>
        public KeyGeneratorStore PrimaryKeyGeneratorStore { get; set; }



        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MapConfig"/> class.
        /// </summary>
        public MapConfig(params IKeyGenerator[] generators)
            : this(
                new ProjectSettings() { InternallyManaged = true, Platform = RdbmsBackend.SupportedSystemNames.Sqlite3 },
                generators)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapConfig" /> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="generators">The generators.</param>
        public MapConfig(ProjectSettings settings, params IKeyGenerator[] generators)
        {
            EntityConfigs = new EntityCollection();
            ComplexTypes = new ComplexTypeCollection();

            MappedStatements = new StatementStore(settings.Platform);
            Settings = settings;

            PrimaryKeyGeneratorStore = new KeyGeneratorStore
            {
                new Generators.GuidCombGenerator(),
                new Generators.AutoIncrementGenerator()
            };

            if (generators == null) return;
            foreach (var keyGenerator in generators)
            {
                PrimaryKeyGeneratorStore.Add(keyGenerator);
            }
        }

        /// <summary>
        /// Gets the entity map.
        /// </summary>
        /// <param name="entityMapFullName">Full name of the entity map.</param>
        /// <returns></returns>
        public EntityMap GetEntityMap(string entityMapFullName)
        {
            EntityMap ent;
            if (!EntityConfigs.TryGetValue(entityMapFullName, out ent))
                throw new MappingException(string.Format("{0} was not found. Probably not mapped", entityMapFullName));

            return ent;
        }


        /// <summary>
        /// Saves model into the specified file as xml.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="readable">if set to <c>true</c> the file will be formated to be readable by humans.</param>
        /// <param name="topSorted">if set to <c>true</c> [top sorted].</param>
        public void Save(string filename, bool readable = false, bool topSorted = false)
        {
            using (var fileStream = File.Open(filename, FileMode.Create, FileAccess.Write))
            {
                Save(fileStream, readable, topSorted);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded { get; internal set; }

        /// <summary>
        /// Loads the mapped statements.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.IO.FileNotFoundException">Cannot load File. File not found.</exception>
        public void LoadMappedStatements(string filename)
        {
            if (!canSetExternalMapStatements)
                throw new InvalidOperationException($"MapConfig not initialized properly. No Platform has been defined. Cannot load statements from {filename}.");

            if (!File.Exists(filename))
                throw new FileNotFoundException("Cannot load File. File not found.", filename);

            using (var filestream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                LoadMappedStatements(filestream);
            }
        }

        /// <summary>
        /// Loads the mapped statements.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <exception cref="System.ArgumentNullException">xmlStream</exception>
        public void LoadMappedStatements(Stream xmlStream)
        {
            if (xmlStream == null) throw new ArgumentNullException("xmlStream");

            var mapConfig = MapConfig.Create(xmlStream);
            LoadMappedStatements(mapConfig.UnprocessedStatements);
        }

        /// <summary>
        /// Loads the mapped statements.
        /// </summary>
        /// <param name="statements">The statements.</param>
        /// <exception cref="System.ArgumentNullException">statements</exception>
        /// <exception cref="System.InvalidOperationException">MapConfig not initialized properly. No Platform has been defined. Cannot load statements.</exception>
        public void LoadMappedStatements(IEnumerable<StatementMap> statements)
        {
            if (statements == null) throw new ArgumentNullException("statements");

            if (!canSetExternalMapStatements)
                throw new InvalidOperationException(
                    "MapConfig not initialized properly. No Platform has been defined. Cannot load statements.");

            foreach (var statementMap in statements)
            {
                MappedStatements.Add(statementMap);
            }
        }


        /// <summary>
        /// Merges the map.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <exception cref="GoliathDataException">
        /// </exception>
        public void MergeMap(MapConfig config)
        {
            foreach (var entity in config.EntityConfigs)
            {
                if (EntityConfigs.Contains(entity.FullName))
                    continue;

                EntityConfigs.Add(entity);
            }

            foreach (var complexType in config.ComplexTypes)
            {
                if (ComplexTypes.Contains(complexType.FullName))
                    continue;
                ComplexTypes.Add(complexType);
            }

            config.MapStatements(config.Settings.Platform);
            MapStatements(Settings.Platform);

            LoadMappedStatements(config.MappedStatements);
        }


        /// <summary>
        /// Loads the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="includeMetadataAttributes">if set to <c>true</c> [include metadata attributes].</param>
        /// <exception cref="System.InvalidOperationException">map config already loaded</exception>
        public void Load(string filename, bool includeMetadataAttributes = false)
        {
            if (IsLoaded)
                throw new InvalidOperationException("map config already loaded");

            if (!File.Exists(filename))
                throw new FileNotFoundException("Cannot load File. File not found.", filename);

            using (var filestream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                Load(filestream, includeMetadataAttributes);
            }
        }

        private bool isSorted;

        /// <summary>
        /// Sorts this instance.
        /// </summary>
        public void Sort()
        {
            if (isSorted || !IsLoaded) return;

            var sorter = new MapSorter();

            logger.Log(LogLevel.Debug, "Applying TopSort");

            var sortedList = sorter.Sort(EntityConfigs);
            EntityConfigs = sortedList;
            isSorted = true;

            logger.Log(LogLevel.Debug, "EntityConfigs sorted.");
        }

        /// <summary>
        /// Loads the specified XML stream.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <param name="includeMetadataAttributes">if set to <c>true</c> [include metadata attributes].</param>
        /// <exception cref="System.InvalidOperationException">map config already loaded</exception>
        public void Load(Stream xmlStream, bool includeMetadataAttributes = false)
        {
            if (IsLoaded)
                throw new InvalidOperationException("map config already loaded");

            using (var reader = XmlReader.Create(xmlStream, new XmlReaderSettings() { IgnoreComments = true, IgnoreWhitespace = true }))
            {
                var mr = new MapReader(includeMetadataAttributes);
                mr.Read(reader, this);
                ProcessAndInspectRelationship();
                IsLoaded = true;
            }
        }

        void ProcessAndInspectRelationship()
        {
            foreach (var entMap in EntityConfigs)
            {
                if (entMap.IsLinkTable)
                {
                    foreach (var pk in entMap.PrimaryKey.Keys)
                    {
                        var rel = pk.Key as Relation;
                        RetrieveAllRelationProperties(entMap, rel);
                    }

                    continue;
                }

                foreach (var rel in entMap.Relations)
                {
                    RetrieveAllRelationProperties(entMap, rel);
                }
            }
        }

        void RetrieveAllRelationProperties(EntityMap entMap, Relation rel)
        {
            if (entMap == null) throw new ArgumentNullException(nameof(entMap));
            if (rel == null) throw new ArgumentNullException(nameof(rel));

            var relEntMap = GetEntityMap(rel.ReferenceEntityName);
            if (relEntMap == null)
                throw new MappingException(string.Format("Could not find Mapped Entity {0} for property {1}.{2}", rel.ReferenceEntityName, entMap.Name, rel.PropertyName));

            rel.ReferenceTable = relEntMap.TableName;
            rel.ReferenceTableSchemaName = relEntMap.SchemaName;

            var refProperty = relEntMap.GetProperty(rel.ReferenceProperty);
            if (refProperty == null)
                throw new MappingException(string.Format("Could not find ReferenceProperty {0} for property {1}.{2}",
                    rel.ReferenceProperty, entMap.Name, rel.PropertyName));

            rel.ReferenceColumn = refProperty.ColumnName;
        }

        private bool canSetExternalMapStatements;

        /// <summary>
        /// Maps the statements.
        /// </summary>
        /// <param name="platform">The platform.</param>
        internal void MapStatements(string platform)
        {
            MappedStatements.SetPlatform(platform);
            Settings.Platform = platform;
            canSetExternalMapStatements = true;

            foreach (var statement in unprocessedStatements)
            {
                MappedStatements.Add(statement);
            }

            unprocessedStatements.Clear();
        }

        /// <summary>
        /// Creates the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="includeMetadataAttributes">if set to <c>true</c> [include metadata attributes].</param>
        /// <param name="generators">The generators.</param>
        /// <returns></returns>
        public static MapConfig Create(string filename, bool includeMetadataAttributes = false, params IKeyGenerator[] generators)
        {
            return Create(filename, null, includeMetadataAttributes, generators);
        }


        /// <summary>
        /// Deserialize file and create a map model.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="includeMetadataAttributes">if set to <c>true</c> [include metadata attributes].</param>
        /// <param name="generators">The generators.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">filename</exception>
        public static MapConfig Create(string filename, ProjectSettings settings, bool includeMetadataAttributes = false, params IKeyGenerator[] generators)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException("filename");

            using (var filestream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                return Create(filestream, settings, includeMetadataAttributes, generators);
            }
        }

        /// <summary>
        /// Creates the specified XML stream.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <param name="includeMetadataAttributes">if set to <c>true</c> [include metadata attributes].</param>
        /// <param name="generators">The generators.</param>
        /// <returns></returns>
        public static MapConfig Create(Stream xmlStream, bool includeMetadataAttributes = false, params IKeyGenerator[] generators)
        {
            return Create(xmlStream, null, includeMetadataAttributes, generators);
        }

        /// <summary>
        /// Deserialize file and create a map model.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="includeMetadataAttributes">if set to <c>true</c> [include metadata attributes].</param>
        /// <param name="generators">The generators.</param>
        /// <returns></returns>
        public static MapConfig Create(Stream xmlStream, ProjectSettings settings, bool includeMetadataAttributes = false, params IKeyGenerator[] generators)
        {
            var config = new MapConfig(generators);
            config.Load(xmlStream, includeMetadataAttributes);

            if (settings != null)
                config.Settings = settings;

            return config;
        }
    }
}

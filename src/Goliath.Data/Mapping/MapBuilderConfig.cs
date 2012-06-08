using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Collections.Generic;

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
        List<StatementMap> unprocessedStatements = new List<StatementMap>();

        internal List<StatementMap> UnprocessedStatements { get { return unprocessedStatements; } }

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
        public MapConfig()
            : this(new ProjectSettings() { InternallyManaged = true, Platform = RdbmsBackend.SupportedSystemNames.Sqlite3 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapConfig"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public MapConfig(ProjectSettings settings)
        {
            EntityConfigs = new EntityCollection();
            ComplexTypes = new ComplexTypeCollection();

            MappedStatements = new StatementStore(settings.Platform);
            Settings = settings;

            PrimaryKeyGeneratorStore = new KeyGeneratorStore();
            PrimaryKeyGeneratorStore.Add(new Generators.GuidCombGenerator());
            PrimaryKeyGeneratorStore.Add(new Generators.AutoIncrementGenerator());
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
        /// Saves the model into the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Save(Stream stream)
        {
            Save(stream, false);
        }

        /// <summary>
        /// Saves model into the specified file as xml.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void Save(string filename)
        {
            Save(filename, false);
        }

        /// <summary>
        /// Saves model into the specified file as xml.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="readable">if set to <c>true</c> the file will be formated to be readable by humans.</param>
        public void Save(string filename, bool readable)
        {
            using (var fileStream = File.Open(filename, FileMode.Create, FileAccess.Write))
            {
                Save(fileStream, readable);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Loads the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void Load(string filename)
        {
            if (IsLoaded)
                throw new InvalidOperationException("map config already loaded");

            using (var filestream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                Load(filestream);
            }
        }

        /// <summary>
        /// Loads the specified XML stream.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        public void Load(Stream xmlStream)
        {
            if (IsLoaded)
                throw new InvalidOperationException("map config already loaded");

            using (XmlReader reader = XmlReader.Create(xmlStream, new XmlReaderSettings() { IgnoreComments = true, IgnoreWhitespace = true }))
            {
                MapReader mr = new MapReader();
                mr.Read(reader, this);
                ProcessAndInspectRelationship();
                //Procedures.platformId = Platform;
                IsLoaded = true;
            }

            //return config;
        }

        void ProcessAndInspectRelationship()
        {
            foreach (var entMap in EntityConfigs)
            {
                if (entMap.IsLinkTable)
                    continue;

                foreach (var rel in entMap.Relations)
                {
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
            }
        }

        /// <summary>
        /// Maps the statements.
        /// </summary>
        /// <param name="platform">The platform.</param>
        internal void MapStatements(string platform)
        {
            MappedStatements.SetPlatform(platform);
            Settings.Platform = platform;

            foreach (var proc in unprocessedStatements)
            {
                MappedStatements.Add(proc);
            }

            unprocessedStatements.Clear();
        }

        /// <summary>
        /// Creates the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static MapConfig Create(string filename)
        {
            return Create(filename, null);
        }


        /// <summary>
        /// Deserialize file and create a map model.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public static MapConfig Create(string filename, ProjectSettings settings)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException("filename");

            using (var filestream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                return Create(filestream, settings);
            }
        }

        /// <summary>
        /// Creates the specified XML stream.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <returns></returns>
        public static MapConfig Create(Stream xmlStream)
        {
            return Create(xmlStream, null);
        }
        /// <summary>
        /// Deserialize file and create a map model.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public static MapConfig Create(Stream xmlStream, ProjectSettings settings)
        {
            MapConfig config;
            if (settings == null)
                config = new MapConfig();
            else
                config = new MapConfig(settings);

            config.Load(xmlStream);
            return config;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Runtime.Serialization;

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
        /// Gets or sets the generated by.
        /// </summary>
        /// <value>The generated by.</value>
        [DataMember]
        public string GeneratedBy { get; set; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        [DataMember]
        public ProjectSettings Settings { get; set; }

        public KeyGeneratorStore PrimaryKeyGeneratorStore { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MapConfig"/> class.
        /// </summary>
        public MapConfig()
        {
            EntityConfigs = new EntityCollection();
            ComplexTypes = new ComplexTypeCollection();
            Settings = new ProjectSettings();
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
        /// Loads the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void Load(string filename)
        {
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
            using (XmlReader reader = XmlReader.Create(xmlStream, new XmlReaderSettings() { IgnoreComments = true, IgnoreWhitespace = true }))
            {
                MapReader mr = new MapReader();
                mr.Read(reader, this);
            }

            //return config;
        }

        /// <summary>
        /// Deserialize file and create a map model.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static MapConfig Create(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException("filename");

            using (var filestream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                return Create(filestream);
            }
        }

        /// <summary>
        /// Deserialize file and create a map model.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <returns></returns>
        public static MapConfig Create(Stream xmlStream)
        {
            MapConfig config = new MapConfig();
            config.Load(xmlStream);
            return config;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class ProjectSettings
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        [DataMember]
        public string Version { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether [support connection reset].
        /// </summary>
        /// <value>
        /// <c>true</c> if [support connection reset]; otherwise, <c>false</c>.
        /// </value>
        public bool SupportConnectionReset { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [support many to many].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [support many to many]; otherwise, <c>false</c>.
        /// </value>
        public bool SupportManyToMany { get; set; } = true;

        public bool GenerateLinkTable { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        [DataMember]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        /// <value>
        /// The namespace.
        /// </value>
        [DataMember]
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
        [DataMember]
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the table prefixes.
        /// </summary>
        /// <value>
        /// The table prefixes.
        /// </value>
        [DataMember]
        public string TablePrefixes { get; set; }

        /// <summary>
        /// Gets or sets the generated by.
        /// </summary>
        /// <value>The generated by.</value>
        [DataMember]
        public string GeneratedBy { get; internal set; }

        internal bool InternallyManaged { get; set; }

        string platform;
        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value>
        /// The platform.
        /// </value>
        public string Platform
        {
            get { return platform; }
            set
            {
                platform = value;
                //Procedures.SetPlatform(value);
            }

        }

        [DataMember] 
        public bool SupportTableInheritance { get; set; } = false;

        /// <summary>
        /// Gets or sets the base model.
        /// </summary>
        /// <value>
        /// The base model.
        /// </value>
        [DataMember]
        public string BaseModel { get; set; }

        public List<string> AdditionalNamespaces { get; set; } = new List<string>();

        readonly Dictionary<string, string> properties = new Dictionary<string, string>();

        internal Dictionary<string, string> Properties { get { return properties; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectSettings" /> class.
        /// </summary>
        /// <param name="supportConnectionResets">if set to <c>true</c> [support connection resets].</param>
        public ProjectSettings(bool supportConnectionResets = false)
        {
            SupportConnectionReset = supportConnectionResets;
        }


        /// <summary>
        /// Tries the get property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetProperty(string propertyName, out string value)
        {
            if (properties.TryGetValue(propertyName, out value))
            {
                return true;
            }

            return false;
        }

        internal void SetPropety(string propertyName, string propertyValue)
        {
            if (properties.ContainsKey(propertyName))
            {
                properties.Remove(propertyName);
            }

            properties.Add(propertyName, propertyValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public struct PropertyNames
        {
            /// <summary>
            /// all dates in this project will be save as UTC date time
            /// </summary>
            public const string SaveAllDateUTC = "Save_all_dates_UTC";
        }

    }
}

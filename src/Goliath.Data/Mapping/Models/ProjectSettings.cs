﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public string Version { get; set; }

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
        /// Gets or sets the base model.
        /// </summary>
        /// <value>
        /// The base model.
        /// </value>
        [DataMember]
        public string BaseModel { get; set; }


        Dictionary<string, string> properties = new Dictionary<string, string>();

        internal Dictionary<string, string> Properties { get { return properties; } }

        public ProjectSettings()
        {
            //SetPropety(ProjectSettings.PropertyNames.SaveAllDateUTC, true);
        }


        /// <summary>
        /// Tries the get property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="prop">The prop.</param>
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

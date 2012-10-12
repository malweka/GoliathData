﻿using System;
using System.Data;
using System.Runtime.Serialization;


namespace Goliath.Data.Mapping
{

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class ConfigProperty : IMapModel, IEquatable<ConfigProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigProperty"/> class.
        /// </summary>
        public ConfigProperty()
        {
            IsNullable = true; 
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        [DataMember]
        public string PropertyName { get; set; }
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        [DataMember]
        public string ColumnName { get; set; }
        /// <summary>
        /// Gets or sets the type of the CLR.
        /// </summary>
        /// <value>The type of the CLR.</value>
        [DataMember]
        public Type ClrType { get; set; }
        /// <summary>
        /// Gets or sets the type of the db.
        /// </summary>
        /// <value>The type of the db.</value>
        [DataMember]
        public DbType DbType { get; set; }
        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        /// <value>The errors.</value>
        [DataMember]
        public string Errors { get; set; }
        /// <summary>
        /// Gets or sets the type of the SQL.
        /// </summary>
        /// <value>The type of the SQL.</value>
        [DataMember]
        public string SqlType { get; set; }
        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>The length.</value>
        [DataMember]
        public int Length { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is nullable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is nullable; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsNullable { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is identity.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is identity; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIdentity { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is auto generated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is auto generated; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsAutoGenerated { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is unique.
        /// </summary>
        /// <value><c>true</c> if this instance is unique; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsUnique { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is primary key.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is primary key; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ColumnName;
        }

        #region IMapModel Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return PropertyName; }
        }

        /// <summary>
        /// Gets the name of the db.
        /// </summary>
        /// <value>The name of the db.</value>
        public string DbName
        {
            get { return ColumnName; }
        }

        #endregion

        #region IEquatable<ConfigProperty> Members

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return PropertyName.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is ConfigProperty)
                return Equals((ConfigProperty)obj);
            return base.Equals(obj);
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(ConfigProperty other)
        {
            if (other == null) return false;

            return ((PropertyName == other.PropertyName) && (ColumnName == other.ColumnName));
        }

        #endregion
    }
}

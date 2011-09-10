using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class ComplexType : IMapModel, IEquatable<ComplexType>
    {
        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        [DataMember]
        public string FullName { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string IMapModel.Name { get { return FullName; } }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enum.
        /// </summary>
        /// <value><c>true</c> if this instance is enum; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsEnum { get; set; }

        /// <summary>
        /// Gets the name of the db.
        /// </summary>
        /// <value>The name of the db.</value>
        string IMapModel.DbName { get { return FullName; } }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The properties.</value>
        [DataMember]
        public PropertyCollection Properties { get; set; }

        public ComplexType(string fullName)
        {
            FullName = fullName;
            Properties = new PropertyCollection();
        }

        #region IEquatable<ComplexTypeConfig> Members

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return FullName.GetHashCode();
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
            if (obj is ComplexType)
                return Equals((ComplexType)obj);

            return base.Equals(obj);
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(ComplexType other)
        {
            if (other == null)
                return false;
            return other.FullName.Equals(FullName);
        }

        #endregion
    }
}

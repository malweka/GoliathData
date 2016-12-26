using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class ComplexType : IEntityMap, IEquatable<ComplexType>
    {
        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        [DataMember]
        public string FullName { get; set; }

        string IMapModel.Name { get { return FullName; } }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enum.
        /// </summary>
        /// <value><c>true</c> if this instance is enum; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsEnum { get; set; }

        string IMapModel.DbName { get { return FullName; } }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The properties.</value>
        [DataMember]
        public PropertyCollection Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexType"/> class.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        public ComplexType(string fullName)
        {
            FullName = fullName;
            Properties = new PropertyCollection();
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public Property GetProperty(string propertyName)
        {
            if (Properties.Contains(propertyName))
                return Properties[propertyName];

            return null;
        }

        /// <summary>
        /// Determines whether the specified property name contains property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        ///   <c>true</c> if the specified property name contains property; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsProperty(string propertyName)
        {
            return Properties.Contains(propertyName);
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

        Property IEntityMap.GetProperty(string propertyName)
        {
            throw new NotImplementedException();
        }

        bool IEntityMap.ContainsProperty(string propertyName)
        {
            throw new NotImplementedException();
        }

        void ICollection<Property>.Add(Property item)
        {
            throw new NotImplementedException();
        }

        void ICollection<Property>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<Property>.Contains(Property item)
        {
            throw new NotImplementedException();
        }

        void ICollection<Property>.CopyTo(Property[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<Property>.Remove(Property item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<Property> IEnumerable<Property>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEntityMap Members

        MapConfig IEntityMap.Parent
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        PrimaryKey IEntityMap.PrimaryKey
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        string IEntityMap.TableName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        string IEntityMap.SchemaName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        //string IEntityMap.TableAlias
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        string IEntityMap.Extends
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        bool IEntityMap.IsLinkTable
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        RelationCollection IEntityMap.Relations
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        IMapModel IEntityMap.BaseModel
        {
            get { throw new NotImplementedException(); }
        }

        PropertyCollection IEntityMap.Properties
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        string IEntityMap.FullName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        int ICollection<Property>.Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICollection<Property>.IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}

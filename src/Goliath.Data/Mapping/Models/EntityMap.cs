using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntityMap : IEntityMap, IEquatable<EntityMap>, IEnumerable<Property>, ICollection<Property>
    {

        MapConfig parent;

        //[IgnoreDataMember]
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public MapConfig Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        string name = string.Empty;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                    return string.Empty;
                else
                    return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets the name of the db.
        /// </summary>
        /// <value>The name of the db.</value>
        public string DbName
        {
            get { return TableName; }
        }

        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        /// <value>The primary key.</value>
        [DataMember]
        public PrimaryKey PrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        /// <value>The namespace.</value>
        [DataMember]
        public string Namespace { get; set; }
        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <value>The name of the assembly.</value>
        [DataMember]
        public string AssemblyName { get; set; }
        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        [DataMember]
        public string TableName { get; set; }
        /// <summary>
        /// Gets or sets the name of the schema.
        /// </summary>
        /// <value>The name of the schema.</value>
        [DataMember]
        public string SchemaName { get; set; }
        /// <summary>
        /// Gets or sets the table alias.
        /// </summary>
        /// <value>The table alias.</value>
        [DataMember]
        public string TableAlias { get; set; }

        string extends;
        /// <summary>
        /// Gets or sets the extends.
        /// </summary>
        /// <value>The extends.</value>
        [DataMember]
        public string Extends
        {
            get { return extends; }
            set
            {
                extends = value;
                baseModel = null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is link table.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is link table; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLinkTable { get; set; }

        internal bool IsSubClass
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Extends) && Parent.EntityConfigs.Contains(Extends))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The properties.</value>
        [DataMember]
        public PropertyCollection Properties { get; set; }

        /// <summary>
        /// Gets or sets the relations.
        /// </summary>
        /// <value>The relations.</value>
        [DataMember]
        public RelationCollection Relations { get; set; }

        /// <summary>
        /// Gets or sets all properties.
        /// </summary>
        /// <value>All properties.</value>
        PropertyCollection AllProperties { get; set; }

        IMapModel baseModel = null;
        /// <summary>
        /// Gets the base model.
        /// </summary>
        /// <value>The base model.</value>
        public IMapModel BaseModel
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Extends))
                    return null;

                if (Parent == null)
                    return null;

                if (baseModel == null)
                {
                    if (Parent.ComplexTypes.Contains(Extends))
                    {
                        baseModel = Parent.ComplexTypes[Extends];
                    }
                    else if (Parent.EntityConfigs.Contains(Extends))
                    {
                        baseModel = parent.EntityConfigs[Extends];
                    }
                }

                return baseModel;
            }
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Namespace))
                    return string.Format("{0}.{1}", Namespace, Name);
                else
                    return Name;
            }
        }

        internal EntityMap() : this(string.Empty, string.Empty) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMap"/> class.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="tableName">Name of the table.</param>
        public EntityMap(string entityName, string tableName)
        {
            Name = entityName;
            TableName = tableName;
            Namespace = string.Empty;
            Properties = new PropertyCollection();
            Relations = new RelationCollection();
            AllProperties = new PropertyCollection();
        }



        /// <summary>
        /// Gets the <see cref="Goliath.Data.Mapping.Property"/> with the specified property name.
        /// </summary>
        public Property this[string propertyName]
        {
            get
            {
                return GetProperty(propertyName);
            }
        }

        /// <summary>
        /// Gets the <see cref="Goliath.Data.Mapping.Property"/> at the specified index.
        /// </summary>
        public Property this[int index]
        {
            get
            {
                return AllProperties[index];
            }
        }

        /// <summary>
        /// Gets the property or relation
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public Property GetProperty(string propertyName)
        {
            if (AllProperties.Contains(propertyName))
                return AllProperties[propertyName];

            return null;
        }

        /// <summary>
        /// Determines whether the specified property name contains property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// 	<c>true</c> if the specified property name contains property; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsProperty(string propertyName)
        {
            return AllProperties.Contains(propertyName);
        }

        #region IEquatable<EntityConfig> Members

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}.{1}, {2}", Namespace, Name, AssemblyName);
        }

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
            if (obj is EntityMap)
                return Equals((EntityMap)obj);

            return base.Equals(obj);
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(EntityMap other)
        {
            if (other == null)
                return false;

            return other.FullName.Equals(FullName);
        }

        #endregion

        #region IEnumerable<Property> Members

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Property> GetEnumerator()
        {
            return AllProperties.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ICollection<Property> Members

        /// <summary>
        /// Adds the column range.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void AddColumnRange(IEnumerable<Property> properties)
        {
            foreach (var prop in properties)
            {
                Add(prop);
            }
        }

        /// <summary>
        /// Adds the key range.
        /// </summary>
        /// <param name="list">The list.</param>
        public void AddKeyRange(IEnumerable<PrimaryKeyProperty> list)
        {
            foreach (var key in list)
                AddKey(key);
        }

        /// <summary>
        /// Adds the key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void AddKey(PrimaryKeyProperty key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (PrimaryKey == null)
                PrimaryKey = new PrimaryKey();

            PrimaryKey.Keys.Add(key);
            AllProperties.Add(key.Key);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(Property item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (item.IsPrimaryKey)
            {
                if (PrimaryKey == null)
                    PrimaryKey = new PrimaryKey();

                PrimaryKey.Keys.Add(new PrimaryKeyProperty(item, string.Empty));
            }

            else if (item is Relation)
                Relations.Add(item as Relation);
            else
                Properties.Add(item);

            AllProperties.Add(item);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            if (PrimaryKey != null)
            {
                PrimaryKey.Keys.Clear();
                PrimaryKey = null;
            }
            Properties.Clear();
            Relations.Clear();
            AllProperties.Clear();
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Property item)
        {
            return AllProperties.Contains(item);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(Property[] array, int arrayIndex)
        {
            AllProperties.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return AllProperties.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Remove(Property item)
        {
            if ((item == null) || string.IsNullOrWhiteSpace(item.PropertyName))
                return false;
            bool res = false;

            if (PrimaryKey != null)
            {
                if (PrimaryKey.Keys.Contains(item.PropertyName))
                {
                    PrimaryKey.Keys.Remove(item.PropertyName);
                    res = true;
                }

            }

            if (Properties.Contains(item.PropertyName))
            {
                Properties.Remove(item.PropertyName);
                res = true;
            }
            else if (Relations.Contains(item.PropertyName))
            {
                Relations.Remove(item.PropertyName);
                res = true;
            }

            if (AllProperties.Contains(item.PropertyName))
            {
                AllProperties.Remove(item.PropertyName);
                res = true;
            }

            return res;
        }

        #endregion
    }
}

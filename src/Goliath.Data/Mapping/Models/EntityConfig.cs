using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{

    [Serializable]
    [DataContract]
    public class EntityMap : IMapModel, IEquatable<EntityMap>, IEnumerable<Property>, ICollection<Property>
    {

        MapConfig parent;

        //[IgnoreDataMember]
        public MapConfig Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        string name = string.Empty;

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

        public string DbName
        {
            get { return TableName; }
        }

        [DataMember]
        public PrimaryKey PrimaryKey { get; set; }

        [DataMember]
        public string Namespace { get; set; }
        [DataMember]
        public string AssemblyName { get; set; }
        [DataMember]
        public string TableName { get; set; }
        [DataMember]
        public string SchemaName { get; set; }
        [DataMember]
        public string TableAbbreviation { get; set; }

        string extends;
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

        [DataMember]
        public bool IsLinkTable { get; set; }

        [DataMember]
        public PropertyCollection Properties { get; set; }
        [DataMember]
        public RelationCollection Relations { get; set; }

        PropertyCollection AllProperties { get; set; }

        IMapModel baseModel = null;
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

        public EntityMap(string entityName, string tableName)
        {
            Name = entityName;
            TableName = tableName;
            Namespace = string.Empty;
            Properties = new PropertyCollection();
            Relations = new RelationCollection();
            AllProperties = new PropertyCollection();
        }

        public void AddColumnRange(IEnumerable<Property> properties)
        {
            foreach (var prop in properties)
            {
                Add(prop);
            }
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
            //if (Properties.Contains(propertyName))
            //    return Properties[propertyName];
            //if (Relations.Contains(propertyName))
            //    return Relations[propertyName];
            //if (PrimaryKey != null)
            //{
            //    if (PrimaryKey.Keys.Contains(propertyName))
            //        return PrimaryKey.Keys[propertyName];
            //}

            if (AllProperties.Contains(propertyName))
                return AllProperties[propertyName];

            return null;
        }

        #region IEquatable<EntityConfig> Members

        public override string ToString()
        {
            return string.Format("{0}.{1}, {2}", Namespace, Name, AssemblyName);
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is EntityMap)
                return Equals((EntityMap)obj);

            return base.Equals(obj);
        }

        public bool Equals(EntityMap other)
        {
            if (other == null)
                return false;

            return other.FullName.Equals(FullName);
        }

        #endregion

        #region IEnumerable<Property> Members

        public IEnumerator<Property> GetEnumerator()
        {
            return AllProperties.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ICollection<Property> Members

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

        public bool Contains(Property item)
        {
            return AllProperties.Contains(item);
        }

        public void CopyTo(Property[] array, int arrayIndex)
        {
            AllProperties.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return AllProperties.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

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

    [Serializable]
    [DataContract]
    public class View : IMapModel
    {
        #region IMapModel Members

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string DbName
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }

    [Serializable]
    [DataContract]
    public class StoredProcedure : IMapModel
    {
        #region IMapModel Members

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string DbName
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}

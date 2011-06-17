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
    public class EntityMap : IMapModel, IEquatable<EntityMap>
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
        }

        public void AddColumnRange(IEnumerable<Property> properties)
        {
            foreach (var prop in properties)
            {
                if (prop is Relation)
                    Relations.Add(prop as Relation);
                else
                    Properties.Add(prop);
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
        /// Gets the property or relation
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public Property GetProperty(string propertyName)
        {
            if (Properties.Contains(propertyName))
                return Properties[propertyName];
            if (Relations.Contains(propertyName))
                return Relations[propertyName];
            if (PrimaryKey != null)
            {
                if (PrimaryKey.Keys.Contains(propertyName))
                    return PrimaryKey.Keys[propertyName];
            }

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

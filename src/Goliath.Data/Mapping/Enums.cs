using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{

    [Serializable]
    [DataContract]
    public enum ConstraintType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        PrimaryKey,
        [EnumMember]
        Unique
    }

    [Serializable]
    [DataContract]
    public enum RelationshipType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ManyToOne = 1,
        [EnumMember]
        OneToMany = 2,
        [EnumMember]
        ManyToMany = 3,
    }

    [Serializable]
    [DataContract]
    public enum CollectionType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        List,
        [EnumMember]
        Map,
        [EnumMember]
        Set,
    }

}

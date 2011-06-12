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
        OneToMany = 1,
        [EnumMember]
        ManyToOne = 2,
        [EnumMember]
        ManyToMany = 3,
    }

}

using System;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public enum ConstraintType
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        None = 0,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        PrimaryKey,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Unique
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public enum RelationshipType
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        None = 0,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        ManyToOne = 1,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        OneToMany = 2,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        ManyToMany = 3,
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public enum CollectionType
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        None = 0,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        List,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Map,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Set,
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public enum MappedStatementType
    {
        /// <summary>
        /// 
        /// </summary>
        Query,
        /// <summary>
        /// 
        /// </summary>
        Update,
        /// <summary>
        /// 
        /// </summary>
        Insert,
        /// <summary>
        /// 
        /// </summary>
        Delete,
        /// <summary>
        /// 
        /// </summary>
        ExecuteNonQuery,
        /// <summary>
        /// 
        /// </summary>
        ExecuteScalar,
    }

}

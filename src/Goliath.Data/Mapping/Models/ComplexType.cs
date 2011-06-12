using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    [Serializable]
    [DataContract]
    public class ComplexType : IMapModel, IEquatable<ComplexType>
    {
        [DataMember]
        public string FullName { get; set; }

        string IMapModel.Name { get { return FullName; } }

        [DataMember]
        public bool IsEnum { get; set; }

        string IMapModel.DbName { get { return FullName; } }

        [DataMember]
        public PropertyCollection Properties { get; set; }

        public ComplexType(string fullName)
        {
            FullName = fullName;
            Properties = new PropertyCollection();
        }

        #region IEquatable<ComplexTypeConfig> Members

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ComplexType)
                return Equals((ComplexType)obj);

            return base.Equals(obj);
        }

        public bool Equals(ComplexType other)
        {
            if (other == null)
                return false;
            return other.FullName.Equals(FullName);
        }

        #endregion
    }
}

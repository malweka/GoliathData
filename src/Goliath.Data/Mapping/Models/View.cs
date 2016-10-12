using System;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class View : IMapModel
    {
        #region IMapModel Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the name of the db.
        /// </summary>
        /// <value>The name of the db.</value>
        public string DbName
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
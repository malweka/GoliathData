using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    //TODO: key generation algorithm
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class PrimaryKey
    {
        /// <summary>
        /// Gets the keys.
        /// </summary>
        [DataMember]
        public PrimaryKeyPropertyCollection Keys { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKey"/> class.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public PrimaryKey(params PrimaryKeyProperty[] keys)
        {
            if (keys != null)
            {
                var list = new PrimaryKeyPropertyCollection();
                list.AddRange(keys);
                Keys = list;
            }
            else
                Keys = new PrimaryKeyPropertyCollection();
        }

        internal PrimaryKey(IList<Property> keys)
        {

            var list = new List<PrimaryKeyProperty>();
            foreach (var k in keys)
            {
                list.Add(k);
            }
            Keys = new PrimaryKeyPropertyCollection();
            Keys.AddRange(list);

        }
    }

   


}

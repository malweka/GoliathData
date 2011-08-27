using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PrimaryKeyPropertyCollection : KeyedCollectionBase<string, PrimaryKeyProperty>// System.Collections.ObjectModel.KeyedCollection<string, PrimaryKeyProperty>
    {
        /// <summary>
        /// Gets the key for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected override string GetKeyForItem(PrimaryKeyProperty item)
        {
            return item.Key.Name;
        }
    }

}

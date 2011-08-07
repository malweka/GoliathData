using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public class PrimaryKeyPropertyCollection : System.Collections.ObjectModel.KeyedCollection<string, PrimaryKeyProperty>
    {
        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="list">The list.</param>
        internal void AddRange(IEnumerable<PrimaryKeyProperty> list)
        {
            foreach (var cmp in list)
            {
                Add(cmp);
            }
        }

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

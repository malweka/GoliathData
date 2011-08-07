using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyGeneratorStore : KeyedCollectionBase<string, IKeyGenerator>, IKeyGeneratorStore
    {
        /// <summary>
        /// Gets the key for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected override string GetKeyForItem(IKeyGenerator item)
        {
            return item.Name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Goliath.Data.Mapping
{
    public class KeyGeneratorStore : KeyedCollectionBase<string, IKeyGenerator>, IKeyGeneratorStore
    {
        protected override string GetKeyForItem(IKeyGenerator item)
        {
            return item.Name;
        }
    }
}

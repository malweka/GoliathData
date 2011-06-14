using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    [Serializable]
    [CollectionDataContract]
    public class EntityCollection : KeyedCollectionBase<string, EntityMap>
    {
        internal void AddRange(IEnumerable<EntityMap> list, MapConfig config)
        {
            foreach (var ent in list)
            {
                ent.Parent = config;
                Add(ent);
            }
        }

        protected override string GetKeyForItem(EntityMap item)
        {
            return item.FullName;
        }
    }

    [Serializable]
    [CollectionDataContract]
    public class ComplexTypeCollection : KeyedCollectionBase<string, ComplexType>
    {
        internal void AddRange(IEnumerable<ComplexType> list, MapConfig config)
        {
            foreach (var cmp in list)
            {
                Add(cmp);
            }
        }

        protected override string GetKeyForItem(ComplexType item)
        {
            return item.FullName;
        }
    }
}

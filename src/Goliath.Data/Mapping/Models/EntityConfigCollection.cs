using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
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

        /// <summary>
        /// Gets the key for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected override string GetKeyForItem(EntityMap item)
        {
            return item.FullName;
        }
    }

    /// <summary>
    /// 
    /// </summary>
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

        /// <summary>
        /// Gets the key for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected override string GetKeyForItem(ComplexType item)
        {
            return item.FullName;
        }
    }
}

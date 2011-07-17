using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Goliath.Data
{
    [Serializable]
    [CollectionDataContract]
    public abstract class KeyedCollectionBase<S, T> : KeyedCollection<S, T>
    {
        protected KeyedCollectionBase()
        {
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="list">The list.</param>
        public void AddRange(IEnumerable<T> list)
        {
            foreach (var t in list)
            {
                Add(t);
            }
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public bool TryGetValue(S key, out T val)
        {
            bool isFound = false;
            val = default(T);

            if (Contains(key))
            {
                isFound = true;
                val = this[key];
            }
            return isFound;
        }
    }
}

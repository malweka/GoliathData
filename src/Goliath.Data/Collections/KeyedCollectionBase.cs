﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [CollectionDataContract]
    public abstract class KeyedCollectionBase<S, T> : KeyedCollection<S, T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedCollectionBase&lt;S, T&gt;"/> class.
        /// </summary>
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
        public virtual bool TryGetValue(S key, out T val)
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

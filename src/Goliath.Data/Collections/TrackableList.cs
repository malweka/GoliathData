using System.Collections.Generic;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TrackableList<T> : IList<T>, ITrackableCollection<T>
    {
        static ILogger logger;
        List<T> deletedItems = new List<T>();
        List<T> insertedItems = new List<T>();
        List<T> list = new List<T>();

        static TrackableList()
        {
            logger = Logger.GetLogger(typeof(LazyList<T>));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackableList&lt;T&gt;"/> class.
        /// </summary>
        public TrackableList()
        {
            //by default we're tracking
            IsTracking = true;
        }

        #region IList<T> Members


        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }


        public void Insert(int index, T item)
        {
            list.Insert(index, item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        ///   
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>The element at the specified index.</returns>
        ///   
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public void Add(T item)
        {
            list.Add(item);

            if (IsTracking)
            {
                if (!insertedItems.Contains(item))
                {
                    insertedItems.Add(item);
                }

                if (deletedItems.Contains(item))
                {
                    deletedItems.Remove(item);
                }
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        public void Clear()
        {
            if (IsTracking)
            {
                insertedItems.Clear();
                foreach (T item in deletedItems)
                {
                    deletedItems.Add(item);
                }
            }

            list.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count
        {
            get { return list.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public bool Remove(T item)
        {
            if (IsTracking)
            {
                if (!deletedItems.Contains(item))
                {
                    deletedItems.Add(item);
                }

                if (insertedItems.Contains(item))
                {
                    insertedItems.Remove(item);
                }
            }

            return list.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator(); ;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region ITrackableCollection<T> Members

        /// <summary>
        /// Starts the tracking.
        /// </summary>
        public void StartTracking()
        {
            IsTracking = true;
        }

        /// <summary>
        /// Stops the tracking.
        /// </summary>
        public void StopTracking()
        {
            IsTracking = false;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is tracking.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is tracking; otherwise, <c>false</c>.
        /// </value>
        public bool IsTracking { get; private set; }

        /// <summary>
        /// Gets the deleted items.
        /// </summary>
        /// <value>The deleted items.</value>
        public ICollection<T> DeletedItems
        {
            get { return deletedItems; }
        }

        /// <summary>
        /// Gets the inserted items.
        /// </summary>
        /// <value>The inserted items.</value>
        public ICollection<T> InsertedItems
        {
            get { return insertedItems; }
        }

        #endregion

        #region ITrackableCollection Members

        System.Collections.IEnumerable ITrackableCollection.DeletedItems
        {
            get { return DeletedItems; }
        }

        System.Collections.IEnumerable ITrackableCollection.InsertedItems
        {
            get { return InsertedItems; }
        }

        #endregion
    }
}

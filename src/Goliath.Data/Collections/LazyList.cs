using System;
using System.Collections.Generic;
using System.Linq;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using Goliath.Data.Sql;

namespace Goliath.Data.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class LazyList<T> : ILazyList<T>, ITrackableCollection<T>
    {
        IList<T> list;
        bool isLoaded;
        private QueryBuilder queryBuilder;
        ISession session;
        EntityMap entityMap;
        IEntitySerializer serializer;
        static ILogger logger;
        

        //create list for tracking that will be helpful for updates of one to many and many to many relations
        List<T> deletedItems = new List<T>();
        List<T> insertedItems = new List<T>();

        public bool IsTracking { get; private set; }

        public ICollection<T> DeletedItems
        {
            get { return deletedItems; }
        }

        public ICollection<T> InsertedItems
        {
            get { return insertedItems; }
        }

        
        static LazyList()
        {
            logger = Logger.GetLogger(typeof(LazyList<T>));
        }

        public LazyList(QueryBuilder queryBuilder, EntityMap entityMap, IEntitySerializer serializer, ISession session)
        {
            
            this.entityMap = entityMap;
            this.serializer = serializer;
            this.session = session;
            this.queryBuilder = queryBuilder;
            IsTracking = true;
            list = new List<T>();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded
        {
            get { return isLoaded; }
        }

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

        void LoadMe()
        {
            if (isLoaded) return;

            logger.Log(LogLevel.Debug, "Opening connection for lazy collection query");
            var sqlbody = queryBuilder.Build();
            var dbConn = session.ConnectionManager.OpenConnection();

            using (
                var dataReader = session.DataAccess.ExecuteReader(dbConn, session.CurrentTransaction, sqlbody.ToString(),
                    queryBuilder.Parameters.ToArray()))
            {
                try
                {
                    list = serializer.SerializeAll<T>(dataReader, entityMap, sqlbody.QueryMap);
                }
                catch (Exception ex)
                {
                    logger.LogException("serialization failed", ex);
                }
            }
                
            isLoaded = true;
            CleanUp();
        }

        void CleanUp()
        {
            if(session!=null)
                session.Dispose();
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
            LoadMe();
            return list.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        ///   
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public void Insert(int index, T item)
        {
            LoadMe();
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
            LoadMe();
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
                LoadMe();
                return list[index];
            }
            set
            {
                LoadMe();
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
            LoadMe();
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
            LoadMe();
            return list.Contains(item);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            LoadMe();
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count
        {
            get
            {
                LoadMe();
                return list.Count;
            }
        }

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
            LoadMe();

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
            LoadMe();
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            LoadMe();
            return list.GetEnumerator();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Goliath.Data.Utils;

namespace Goliath.Data.Entity
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ChangeSet
    {
        Dictionary<string, ChangeItem> changeList = new Dictionary<string, ChangeItem>();

        /// <summary>
        /// Gets the version.
        /// </summary>
        public long Version { get; private set; }
        object stateObject = new object();
        List<string> changes = new List<string>();

        bool tracking;

        /// <summary>
        /// Starts the tracking.
        /// </summary>
        public void StartTracking()
        {
            tracking = true;
        }

        /// <summary>
        /// Stops the tracking.
        /// </summary>
        public void StopTracking()
        {
            tracking = false;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has changes.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
        /// </value>
        public bool HasChanges
        {
            get
            {
                bool hasChanges;
                lock (stateObject)
                {
                    hasChanges = (changes.Count > 0);
                }
                return hasChanges;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeSet"/> class.
        /// </summary>
        public ChangeSet()
        {
            Version = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Gets the changed items.
        /// </summary>
        /// <returns></returns>
        public ICollection<ChangeItem> GetChangedItems()
        {
            ChangeItem[] items;
            lock (stateObject)
            {
                items = changeList.Values.Where(c => c.Version > Version).ToArray();
            }
            return items;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            lock (stateObject)
            {
                Version = DateTime.Now.Ticks;
                foreach (var item in changeList.Values)
                {
                    item.InitialValue = item.Value;
                    item.Version = Version;
                }
                changes.Clear();
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            lock (stateObject)
            {
                changes.Clear();
                changeList.Clear();
            }
        }

        /// <summary>
        /// Tracks the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public void Track(string propertyName, object value)
        {
            if (!tracking)
                return;

            lock (stateObject)
            {
                ChangeItem item;
                if (changeList.TryGetValue(propertyName, out item))
                {
                    if (item.Value.Equals(value))
                    {
                        return;
                    }

                    item.Value = value;
                    if (item.InitialValue.Equals(value))
                    {
                        item.Version = Version;
                        if (changes.Contains(propertyName))
                            changes.Remove(propertyName);
                    }
                    else
                    {
                        item.Version = DateTime.Now.Ticks;
                        if (!changes.Contains(propertyName))
                            changes.Add(propertyName);
                    }
                }
                else
                {
                    item = new ChangeItem(propertyName, value) { Version = Version };
                    changeList.Add(propertyName, item);
                }
            }
        }

        /// <summary>
        /// Tracks the specified property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        public void Track<TProperty>(Expression<Func<TProperty>> property, object value)
        {
            var propertyName = property.GetMemberName();
            Track(propertyName, value);
        }

    }
}

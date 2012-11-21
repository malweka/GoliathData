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
    public class ChangeTracker : IChangeTracker
    {
        readonly Dictionary<string, TrackedItem> changeList = new Dictionary<string, TrackedItem>();
        readonly List<string> changes = new List<string>();
        bool tracking;

        /// <summary>
        /// Gets the version.
        /// </summary>
        public long Version { get; private set; }

        /// <summary>
        /// Starts the tracking.
        /// </summary>
        public void Start()
        {
            tracking = true;
        }

        /// <summary>
        /// Pauses the tracking.
        /// </summary>
        public void Pause()
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
                var hasChanges = (changes.Count > 0);
                return hasChanges;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeTracker" /> class.
        /// </summary>
        public ChangeTracker()
        {
            Version = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Loads the intial values.
        /// </summary>
        /// <param name="initialValues">The initial values.</param>
        public void LoadIntialValues(Tuple<string, object>[] initialValues)
        {
            if (tracking)
                return;

            foreach (var tuple in initialValues)
            {
                if (!string.IsNullOrWhiteSpace(tuple.Item1))
                {
                    changeList.Add(tuple.Item1, new TrackedItem(tuple.Item1, tuple.Item2));
                }
            }
        }

        /// <summary>
        /// Gets the changed items.
        /// </summary>
        /// <returns></returns>
        public ICollection<ITrackedItem> GetChangedItems()
        {
            var items = changeList.Values.Where(c => c.Version > Version).ToArray();
            return items;
        }

        /// <summary>
        /// Resets all the changes.
        /// </summary>
        public void Reset()
        {
            Version = DateTime.Now.Ticks;
            foreach (var item in changeList.Values)
            {
                item.InitialValue = item.Value;
                item.Version = Version;
            }
            changes.Clear();

        }

        /// <summary>
        /// Clears changes.
        /// </summary>
        public void Clear()
        {
            changes.Clear();
            changeList.Clear();
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

            TrackedItem item;
            if (changeList.TryGetValue(propertyName, out item))
            {
                if (item.Value.Equals(value))
                {
                    return;
                }

                item.Value = value;
                if (((item.InitialValue != null) && item.InitialValue.Equals(value)) || ((item.InitialValue == null) && (value == null)))
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
                item = new TrackedItem(propertyName, value) { Version = Version };
                changeList.Add(propertyName, item);
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

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

        #region Properties and variables

        readonly Dictionary<string, TrackedItem> changeList = new Dictionary<string, TrackedItem>();
        readonly List<string> changes = new List<string>();
        private Func<IDictionary<string, object>> getInitialValuesMethod;
        bool tracking;
        private bool isInitialized;

        /// <summary>
        /// Gets a value indicating whether this instance is tracking.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tracking; otherwise, <c>false</c>.
        /// </value>
        public bool IsTracking
        {
            get { return tracking; }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public long Version { get; private set; }

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
        #endregion

        public void Init()
        {
            if (isInitialized) return;

            InitializeTrackList(getInitialValuesMethod());
            isInitialized = true;
        }

        /// <summary>
        /// Starts the tracking.
        /// </summary>
        public void Start()
        {
            tracking = true;
            Init();
        }

        /// <summary>
        /// Pauses the tracking.
        /// </summary>
        public void Pause()
        {
            tracking = false;
        }

        void UpdateVersion()
        {
            Version = DateTime.UtcNow.Ticks;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeTracker"/> class.
        /// </summary>
        /// <param name="getInitialValuesMethod">The get initial values method.</param>
        public ChangeTracker(Func<IDictionary<string, object>> getInitialValuesMethod)
        {
            if (getInitialValuesMethod == null) throw new ArgumentNullException("getInitialValuesMethod");
            this.getInitialValuesMethod = getInitialValuesMethod;
            UpdateVersion();
        }

        void InitializeTrackList(IDictionary<string, object> initialValues)
        {
            if (tracking)
                return;

            foreach (var tuple in initialValues)
            {
                if (!string.IsNullOrWhiteSpace(tuple.Key) && !changeList.ContainsKey(tuple.Key))
                {
                    changeList.Add(tuple.Key, new TrackedItem(tuple.Key, tuple.Value));
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
        public void CommitChanges()
        {
            UpdateVersion();
            foreach (var item in changeList.Values)
            {
                item.InitialValue = item.Value;
                item.Version = Version;
                item.Value = null;
            }

            changes.Clear();
        }

        /// <summary>
        /// Clears changes.
        /// </summary>
        public void StopAndClear()
        {
            changes.Clear();
            changeList.Clear();
            isInitialized = false;
            tracking = false;
        }

        /// <summary>
        /// Loads the initial value.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool LoadInitialValue(string propertyName, object value)
        {
            if (!tracking)
            {
                TrackedItem item;
                if (changeList.TryGetValue(propertyName, out item))
                {
                    item.Value = value;
                    item.Version = Version;
                    return true;
                }
            }

            return false;
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
                if (object.Equals(item.Value, value))//(item.Value.Equals(value))
                {
                    return;
                }

                item.Value = value;
                if (((item.InitialValue != null) && object.Equals(item.InitialValue, value)) || ((item.InitialValue == null) && (value == null)))
                {
                    item.Version = Version;
                    if (changes.Contains(propertyName))
                        changes.Remove(propertyName);
                }
                else
                {
                    item.Version = DateTime.UtcNow.Ticks;
                    if (!changes.Contains(propertyName))
                        changes.Add(propertyName);
                }
            }
            //else
            //{
            //    item = new TrackedItem(propertyName, value) { Version = Version };
            //    changeList.Add(propertyName, item);
            //}
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

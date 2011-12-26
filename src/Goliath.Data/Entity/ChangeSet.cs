using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Goliath.Data.Entity
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ChangeItem
    {
        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        /// <value>
        /// The name of the item.
        /// </value>
        public string ItemName { get; private set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; set; }
        /// <summary>
        /// Gets the initial value.
        /// </summary>
        public object InitialValue { get; internal set; }
        /// <summary>
        /// Gets the version.
        /// </summary>
        public long Version { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeItem"/> class.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="initialValue">The initial value.</param>
        public ChangeItem(string itemName, object initialValue)
        {
            ItemName = itemName;
            InitialValue = initialValue;
            Value = initialValue;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ChangeSet
    {
        Dictionary<string, ChangeItem> changeList = new Dictionary<string, ChangeItem>();
        public long Version { get; private set; }
        object stateObject = new object();
        List<string> changes = new List<string>();

        bool tracking = false;

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

            ChangeItem item;
            lock (stateObject)
            {
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
            string propertyName = GetMemberName(property);
            Track(propertyName, value);
        }

        string GetMemberName<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else memberExpression = (MemberExpression)lambda.Body;

            return memberExpression.Member.Name;
        }

    }
}

using System;

namespace Goliath.Data.Entity
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TrackedItem : ITrackedItem
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
        /// Initializes a new instance of the <see cref="TrackedItem"/> class.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="initialValue">The initial value.</param>
        public TrackedItem(string itemName, object initialValue)
        {
            ItemName = itemName;
            InitialValue = initialValue;
            Value = initialValue;
        }
    }
}
namespace Goliath.Data.Entity
{
    public interface ITrackedItem
    {
        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        /// <value>
        /// The name of the item.
        /// </value>
        string ItemName { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        object Value { get; set; }

        /// <summary>
        /// Gets the initial value.
        /// </summary>
        object InitialValue { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        long Version { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Collections
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITrackableCollection<T> : ICollection<T>
    {
        /// <summary>
        /// Starts the tracking.
        /// </summary>
        void StartTracking();

        /// <summary>
        /// Stops the tracking.
        /// </summary>
        void StopTracking();

        /// <summary>
        /// Gets a value indicating whether this instance is tracking.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is tracking; otherwise, <c>false</c>.
        /// </value>
        bool IsTracking { get; }

        /// <summary>
        /// Gets the deleted items.
        /// </summary>
        /// <value>The deleted items.</value>
        ICollection<T> DeletedItems { get; }

        /// <summary>
        /// Gets the inserted items.
        /// </summary>
        /// <value>The inserted items.</value>
        ICollection<T> InsertedItems { get; }
    }
}

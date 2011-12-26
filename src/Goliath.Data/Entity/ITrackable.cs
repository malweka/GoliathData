using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Entity
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITrackable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is dirty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is dirty; otherwise, <c>false</c>.
        /// </value>
        bool IsDirty { get; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        long Version { get; set; }

        /// <summary>
        /// Gets the created on.
        /// </summary>
        DateTime CreatedOn { get; }
    }
}

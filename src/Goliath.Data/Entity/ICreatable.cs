using System;

namespace Goliath.Data.Entity
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICreatable
    {
        /// <summary>
        /// Gets or sets the created on.
        /// </summary>
        /// <value>
        /// The created on.
        /// </value>
        DateTime CreatedOn { get; set; }
        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        /// <value>
        /// The created by.
        /// </value>
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IModifiable
    {
        /// <summary>
        /// Gets or sets the modified on.
        /// </summary>
        /// <value>
        /// The modified on.
        /// </value>
        DateTime? ModifiedOn { get; set; }
        /// <summary>
        /// Gets or sets the modified by.
        /// </summary>
        /// <value>
        /// The modified by.
        /// </value>
        string ModifiedBy { get; set; }
    }
}
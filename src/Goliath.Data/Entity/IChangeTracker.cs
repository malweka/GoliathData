using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Goliath.Data.Entity
{
    public interface IChangeTracker
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        long Version { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has changes.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
        /// </value>
        bool HasChanges { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is tracking.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tracking; otherwise, <c>false</c>.
        /// </value>
        bool IsTracking { get; }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        void Init();

        /// <summary>
        /// Loads the initial value.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        bool LoadInitialValue(string propertyName, object value);

        /// <summary>
        /// Starts the tracking.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the tracking.
        /// </summary>
        void Pause();

        /// <summary>
        /// Gets the changed items.
        /// </summary>
        /// <returns></returns>
        ICollection<ITrackedItem> GetChangedItems();

        /// <summary>
        /// Resets this instance.
        /// </summary>
        void Reset();

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void StopAndClear();

        ///// <summary>
        ///// Loads the intial values.
        ///// </summary>
        ///// <param name="initialValues">The initial values.</param>
        //void LoadIntialValues(Tuple<string, object>[] initialValues);

        /// <summary>
        /// Tracks the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        void Track(string propertyName, object value);

        /// <summary>
        /// Tracks the specified property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        void Track<TProperty>(Expression<Func<TProperty>> property, object value);
    }
}
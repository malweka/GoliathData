using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Goliath.Data.Utils
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PropertyAccessor
    {
        /// <summary>
        /// Gets the type of the declaring.
        /// </summary>
        /// <value>
        /// The type of the declaring.
        /// </value>
        public Type DeclaringType { get; internal set; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>
        /// The type of the property.
        /// </value>
        public Type PropertyType { get; internal set; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; internal set; }

        /// <summary>
        /// Gets the get method.
        /// </summary>
        /// <value>
        /// The get method.
        /// </value>
        public Func<object, object> GetMethod { get; internal set; }

        /// <summary>
        /// Gets the set method.
        /// </summary>
        /// <value>
        /// The set method.
        /// </value>
        public Action<object, object> SetMethod { get; internal set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryParam
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParam"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public QueryParam(string name) : this(name, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParam"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public QueryParam(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            Name=name;
            Value=value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryParam<T> : QueryParam
    {
        public QueryParam(string name) : base(name) { }
        public QueryParam(string name, T value)
            : base(name)
        {
            Value = value;
        }

        public new T Value { get; set; }
    }
}

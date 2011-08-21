﻿using System;
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
        public string Name { get; protected set; }
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

            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PropertyQueryParam : QueryParam
    {
        public Sql.ComparisonOperator ComparisonOperator { get; set; }
        public Sql.SqlOperator PostOperator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyQueryParam"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        public PropertyQueryParam(string propertyName) : this(propertyName, null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyQueryParam"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        public PropertyQueryParam(string propertyName, object value)
            : base(propertyName, value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException("propertyName");

            PropertyName = propertyName;
            ComparisonOperator = Sql.ComparisonOperator.Equal;
            PostOperator = Sql.SqlOperator.AND;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; private set; }

        internal void SetParameterName(string columnName, string tableAlias)
        {
            Name = ParameterNameBuilderHelper.ColumnQueryName(columnName, tableAlias);
        }
    }
}

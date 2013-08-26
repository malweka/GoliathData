using System;
using Goliath.Data.Sql;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Prop = {PropertyName}, Value = {Value}")]
    public class PropertyQueryParam : QueryParam
    {
        /// <summary>
        /// Gets or sets the comparison operator.
        /// </summary>
        /// <value>
        /// The comparison operator.
        /// </value>
        public ComparisonOperator ComparisonOperator { get; set; }

        /// <summary>
        /// Gets or sets the post operator.
        /// </summary>
        /// <value>
        /// The post operator.
        /// </value>
        public SqlOperator PostOperator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyQueryParam"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public PropertyQueryParam(string propertyName) : this(propertyName, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyQueryParam"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
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
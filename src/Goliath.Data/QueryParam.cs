using System;
using System.Data;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Param = {Name}, Value = {Value}")]
    public class QueryParam
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; protected set; }

        public DbType? PropertyDbType { get; private set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public virtual object Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParam" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="expectedType">The expected type.</param>
        public QueryParam(string name, DbType? dbType) : this(name, null, dbType) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParam" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public QueryParam(string name, object value, DbType? dbType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            Name = name;
            Value = value;
            PropertyDbType = dbType;
        }

        public override string ToString()
        {
            return Name;
        }

        internal static QueryParam CreateParameter(Mapping.Property property, string paramName, object value)
        {
            if ((value != null) && property.IsComplexType && value.GetType().IsEnum &&
                ((property.DbType == DbType.String) || (property.DbType == DbType.AnsiString) || (property.DbType == DbType.AnsiStringFixedLength) || (property.DbType == DbType.StringFixedLength)))
            {
                return new QueryParam(paramName, value.ToString(), property.DbType);
            }
            else return new QueryParam(paramName, value, property.DbType);
        }
    }
}

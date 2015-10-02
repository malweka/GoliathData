namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntityMap : IMapModel
    {
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        MapConfig Parent { get; set; }

        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        /// <value>
        /// The primary key.
        /// </value>
        PrimaryKey PrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        string TableName { get; set; }

        /// <summary>
        /// Gets or sets the name of the schema.
        /// </summary>
        /// <value>
        /// The name of the schema.
        /// </value>
        string SchemaName { get; set; }

        ///// <summary>
        ///// Gets or sets the table alias.
        ///// </summary>
        ///// <value>
        ///// The table alias.
        ///// </value>
        //string TableAlias { get; set; }

        /// <summary>
        /// Gets or sets the extends.
        /// </summary>
        /// <value>
        /// The extends.
        /// </value>
        string Extends { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is link table.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is link table; otherwise, <c>false</c>.
        /// </value>
        bool IsLinkTable { get; set; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        PropertyCollection Properties { get; set; }

        /// <summary>
        /// Gets or sets the relations.
        /// </summary>
        /// <value>
        /// The relations.
        /// </value>
        RelationCollection Relations { get; set; }

        /// <summary>
        /// Gets the base model.
        /// </summary>
        /// <value>
        /// The base model.
        /// </value>
        IMapModel BaseModel { get; }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        string FullName { get; }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        Property GetProperty(string propertyName);

        /// <summary>
        /// Determines whether the specified property name contains property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        bool ContainsProperty(string propertyName);
    }
}
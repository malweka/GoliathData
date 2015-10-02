
namespace Goliath.Data.Mapping
{
    /// <summary>
    /// Map model contract
    /// </summary>
    public interface IMapModel
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
        /// <summary>
        /// Gets the name of the db.
        /// </summary>
        /// <value>The name of the db.</value>
        string DbName { get; }
    }

    public interface IEntityMap : IMapModel
    {
        MapConfig Parent { get; set; }
        PrimaryKey PrimaryKey { get; set; }
        string TableName { get; set; }
        string SchemaName { get; set; }
        string TableAlias { get; set; }
        string Extends { get; set; }
        bool IsLinkTable { get; set; }
        PropertyCollection Properties { get; set; }
        RelationCollection Relations { get; set; }
        IMapModel BaseModel { get; }
        string FullName { get; }

        Property GetProperty(string propertyName);
        bool ContainsProperty(string propertyName);
    }
}


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
}

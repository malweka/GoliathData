using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Mapping
{
    using Providers;
    /// <summary>
    /// 
    /// </summary>
    public interface IKeyGenerator
    {
        Type KeyType { get; }
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
        /// <summary>
        /// Generates the key.
        /// </summary>
        /// <param name="sqlMapper">The SQL mapper.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        Object GenerateKey(SqlMapper sqlMapper, EntityMap entityMap, string propertyName, out Sql.SqlOperationPriority priority);
        /// <summary>
        /// Gets a value indicating whether this instance is database generated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is database generated; otherwise, <c>false</c>.
        /// </value>
        bool IsDatabaseGenerated { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IKeyGenerator<T> : IKeyGenerator
    {
        /// <summary>
        /// Generates this instance.
        /// </summary>
        /// <param name="sqlMapper">The SQL mapper.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        T Generate(SqlMapper sqlMapper, EntityMap entityMap, string propertyName, out Sql.SqlOperationPriority priority);
    }

    public interface IKeyGeneratorStore
    {
        void Add(IKeyGenerator generator);
        IKeyGenerator this[string keyGeneratorName] { get; }
    }
}

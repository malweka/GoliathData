using System;

namespace Goliath.Data.Mapping
{
    using Providers;
    /// <summary>
    /// 
    /// </summary>
    public interface IKeyGenerator
    {
        /// <summary>
        /// Gets the type of the key.
        /// </summary>
        /// <value>
        /// The type of the key.
        /// </value>
        Type KeyType { get; }
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Generates the key.
        /// </summary>
        /// <param name="sqlDialect">The SQL dialect.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        Object GenerateKey(SqlDialect sqlDialect, EntityMap entityMap, string propertyName, out Sql.SqlOperationPriority priority);
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
        /// Generates the specified SQL dialect.
        /// </summary>
        /// <param name="sqlDialect">The SQL dialect.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        T Generate(SqlDialect sqlDialect, EntityMap entityMap, string propertyName, out Sql.SqlOperationPriority priority);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IKeyGeneratorStore
    {
        /// <summary>
        /// Adds the specified generator.
        /// </summary>
        /// <param name="generator">The generator.</param>
        void Add(IKeyGenerator generator);
        /// <summary>
        /// Gets the <see cref="Goliath.Data.Mapping.IKeyGenerator"/> with the specified key generator name.
        /// </summary>
        IKeyGenerator this[string keyGeneratorName] { get; }
    }
}

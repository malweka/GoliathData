using Goliath.Data.Mapping;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISqlWorker
    {
        /// <summary>
        /// Builds the insert SQL.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        BatchSqlOperation BuildInsertSql<TEntity>(EntityMap entityMap, TEntity entity, bool recursive);

        /// <summary>
        /// Builds the update SQL.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="updateManyToManyRelation">if set to <c>true</c> [update many to many relation].</param>
        /// <returns></returns>
        BatchSqlOperation BuildUpdateSql<TEntity>(EntityMap entityMap, TEntity entity, bool updateManyToManyRelation = false);

        /// <summary>
        /// Builds the delete SQL.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="cascade">if set to <c>true</c> [cascade].</param>
        /// <returns></returns>
        BatchSqlOperation BuildDeleteSql<TEntity>(EntityMap entityMap, TEntity entity, bool cascade);
    }
}

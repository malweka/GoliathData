using System;
using Goliath.Data.Mapping;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntityFactory
    {
        /// <summary>
        /// Creates the new instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateNewInstance<T>();
        /// <summary>
        /// Creates the new instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        T CreateNewInstance<T>(EntityMap entityMap);
        /// <summary>
        /// Creates the new instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        object CreateNewInstance(Type type, EntityMap entityMap);
    }
}

using Goliath.Data.Mapping;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntityFactory
    {
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateInstance<T>();

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateInstance<T>(EntityMap entityMap);
    }
}

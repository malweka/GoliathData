using System.Data.Common;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataHydrator<T>
    {
        /// <summary>
        /// Serializes from data reader.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        T HydrateFromDataReader(DbDataReader dataReader, Mapping.EntityMap entityMap);
    }
}

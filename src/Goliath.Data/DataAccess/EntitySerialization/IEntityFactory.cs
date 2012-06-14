using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.DataAccess
{
    using Mapping;

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

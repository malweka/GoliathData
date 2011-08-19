using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Goliath.Data
{
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Goliath.Data
{
    public interface IEntitySerializer<T> : IEntitySerializer
    {
        /// <summary>
        /// Serializes from data reader.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        T SerializeFromDataReader(DbDataReader dataReader, Mapping.EntityMap entityMap);
    }

    public interface IEntitySerializer
    {
        Type Type { get; }
        object SerializeFromDataReader(DbDataReader dataReader, Mapping.EntityMap entityMap);
    }
}

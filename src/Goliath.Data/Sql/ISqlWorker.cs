using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Mapping;

namespace Goliath.Data.Sql
{
    public interface ISqlWorker
    {
        /// <summary>
        /// Builds the insert SQL.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        BatchSqlOperation BuildInsertSql<TEntity>(EntityMap entityMap, TEntity entity, bool recursive);
    }
}

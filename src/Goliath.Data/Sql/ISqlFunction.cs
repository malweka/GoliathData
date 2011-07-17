using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// Sql Function representation
    /// </summary>
    public interface ISqlFunction
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
        /// <summary>
        /// Gets the function declaration.
        /// </summary>
        string Declaration { get; }
        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <value>The type of the return.</value>
        Type ReturnType { get; }
        /// <summary>
        /// Toes the SQL statement.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        string ToSqlStatement(params QueryParam[] args);
    }
}

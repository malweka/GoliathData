using System;
using System.Collections.Generic;
using System.Text;
using Goliath.Data.Providers;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateSqlBodyInfo
    {
        readonly Dictionary<string, Tuple<QueryParam,bool>> columns = new Dictionary<string, Tuple<QueryParam, bool>>();
        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public Dictionary<string, Tuple<QueryParam, bool>> Columns { get { return columns; } }

        /// <summary>
        /// Gets or sets the into.
        /// </summary>
        /// <value>
        /// The into.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the where expression.
        /// </summary>
        /// <value>The where expression.</value>
        public string WhereExpression { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="dialect">The dialect.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(SqlDialect dialect)
        {
            return dialect.BuildUpdateStatement(this);
        }

    }
}
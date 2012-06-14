using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    using Providers;
    using Mapping;

    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ColumnEnumeration} {From} {JoinEnumeration} ...")]
    public struct SqlQueryBody
    {
        /// <summary>
        /// Gets or sets the column enumeration.
        /// </summary>
        /// <value>The column enumeration.</value>
        public string ColumnEnumeration { get; set; }

        /// <summary>
        /// Gets or sets from.
        /// </summary>
        /// <value>From.</value>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the join enumeration.
        /// </summary>
        /// <value>The join enumeration.</value>
        public string JoinEnumeration { get; set; }

        /// <summary>
        /// Gets or sets the where expression.
        /// </summary>
        /// <value>The where expression.</value>
        public string WhereExpression { get; set; }

        /// <summary>
        /// Gets or sets the sort expression.
        /// </summary>
        /// <value>The sort expression.</value>
        public string SortExpression { get; set; }

        /// <summary>
        /// Gets or sets the aggregate expression.
        /// </summary>
        /// <value>The aggregate expression.</value>
        public string AggregateExpression { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="dialect">The dialect.</param>
        /// <param name="paging">The paging.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(SqlDialect dialect, PagingInfo paging)
        {
            return dialect.QueryWithPaging(this, paging);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("SELECT ");
            sb.Append(ColumnEnumeration);
            sb.AppendFormat("\nFROM {0}", From);

            if (!string.IsNullOrWhiteSpace(JoinEnumeration))
                sb.Append(JoinEnumeration);
            if (!string.IsNullOrWhiteSpace(WhereExpression))
                sb.AppendFormat("\nWHERE {0}\n", WhereExpression);

            return sb.ToString();
        }
    }
}

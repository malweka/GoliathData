using System.Text;
using Goliath.Data.Providers;

namespace Goliath.Data
{
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
            sb.AppendFormat(" FROM {0} ", From);

            if (!string.IsNullOrWhiteSpace(JoinEnumeration))
            {
                sb.AppendFormat("{0} ", JoinEnumeration);
            }

            if (!string.IsNullOrWhiteSpace(WhereExpression))
            {
                sb.AppendFormat("WHERE {0} ", WhereExpression);
            }

            if (!string.IsNullOrWhiteSpace(SortExpression))
            {
                sb.AppendFormat("ORDER BY {0}", SortExpression);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct PagingInfo
    {
        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public int Total { get; set; }
        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        /// <value>
        /// The limit.
        /// </value>
        public int Limit { get; set; }
        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public int Offset { get; set; }
    }
}

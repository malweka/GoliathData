using System.Collections.Generic;
using Goliath.Data.Providers;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class DeleteSqlBodyInfo
    {
        readonly List<QueryParam> parameters = new List<QueryParam>();
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public List<QueryParam> Parameters { get { return parameters; } }

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
            return dialect.BuildDeleteStatement(this);
        }
    }
}
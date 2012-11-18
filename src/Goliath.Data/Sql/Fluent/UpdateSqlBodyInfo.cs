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
        /// <summary>
        /// Gets or sets the column enumeration.
        /// </summary>
        /// <value>The column enumeration.</value>
        public List<string> ColumnList { get; set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public List<QueryParam> Parameters { get; set; }

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
            var sb = new StringBuilder("UPDATE ");
            sb.AppendFormat("{0} SET ", dialect.Escape(TableName, EscapeValueType.TableName));
            try
            {
                for (int i = 0; i < ColumnList.Count; i++)
                {
                    sb.AppendFormat("{0} = {1}", dialect.Escape(ColumnList[i], EscapeValueType.Column), dialect.CreateParameterName(Parameters[i].Name));
                    if(i < (ColumnList.Count - 1))
                        sb.Append(", ");
                }
            }
            catch (Exception exception)
            {
                throw new GoliathDataException("Couldn't not build update statement. Please verify that you have matchin parameters for all your columns.", exception);
            }

            sb.AppendFormat(" WHERE {0}", WhereExpression);
            return sb.ToString();
        }
    }
}
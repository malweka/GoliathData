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
            var sb = new StringBuilder("UPDATE ");
            sb.AppendFormat("{0} SET ", dialect.Escape(TableName, EscapeValueType.TableName));
            try
            {
                int counter = 0;
                foreach(var col in Columns)
                {
                    sb.AppendFormat("{0} = {1}", dialect.Escape(col.Key, EscapeValueType.Column), dialect.CreateParameterName(col.Value.Item1.Name));
                    if(counter < (Columns.Count - 1))
                        sb.Append(", ");

                    counter = counter + 1;
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
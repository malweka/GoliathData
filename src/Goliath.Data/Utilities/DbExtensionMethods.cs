using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Goliath.Data
{
    public static class DbExtensionMethods
    {
        /// <summary>
        /// Gets the string value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static string GetValueAsString(this DbDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader[columnName] == DBNull.Value)
                return null;
            else
                return reader[columnName].ToString();
        }

        public static int? GetValueAsInt(this DbDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader[columnName] == DBNull.Value)
                return null;
            else
            {
                int rv;
                if (int.TryParse(reader[columnName].ToString(), out rv))
                {
                    return rv;
                }
                else
                    return null;
            }
        }

        public static long? GetValueAsLong(this DbDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (reader == null)
                throw new ArgumentNullException("reader");

            var data = reader[columnName];
            if (data == DBNull.Value)
                return null;
            else
            {
                if (data is long)
                    return (long)data;
                else
                {
                    var val = Convert.ToInt64(data.ToString());
                    return val;
                }
            }
        }

        public static object GetValueAsObject(this DbDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader[columnName] == DBNull.Value)
                return null;
            else
                return reader[columnName];
        }
    }
}

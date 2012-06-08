using System;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public static class DataAccessHelper
    {
        
        /// <summary>
        /// Reads the enum.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns></returns>
        public static object ReadEnum(this DbDataReader reader, string columnName, Type enumType)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (reader == null)
                throw new ArgumentNullException("reader");

            try
            {
                Object enumVal = null;
                if (reader[columnName] is string)
                {
                    enumVal = Enum.Parse(enumType, reader[columnName].ToString(), true);
                }
                else if ((reader[columnName] is Int16) || (reader[columnName] is Int32) || (reader[columnName] is Int64))
                {
                    enumVal = Enum.ToObject(enumType, reader[columnName]);
                }

                return enumVal;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(string.Format("Could not convert field value {0} to type of {1}",
                    columnName, enumType), ex);
            }
            //return default(TEnum);
        }
    }
}

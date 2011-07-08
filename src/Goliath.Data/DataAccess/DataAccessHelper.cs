using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    public static class DataAccessHelper
    {
        public static string ReadString(this DbDataReader reader, string columnName)
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

        public static int? ReadNullableInt(this DbDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader[columnName] == DBNull.Value)
                return null;
            else
            {
                try
                {
                    int rv = Convert.ToInt32(reader[columnName]);
                    return rv;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                return null;
            }
        }

        public static int ReadInt(this DbDataReader reader, string columnName)
        {
            int? i = ReadNullableInt(reader, columnName);
            if (i.HasValue)
                return i.Value;
            else
                return default(int);
        }

        public static DateTime ReadDateTime(this DbDataReader reader, string columnName)
        {
            DateTime? date = ReadNullableDateTime(reader, columnName);
            if (date.HasValue)
                return date.Value;
            else
                return default(DateTime);
        }

        public static DateTime? ReadNullableDateTime(this DbDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader[columnName] == DBNull.Value)
                return null;
            else
            {
                try
                {
                    DateTime rv;
                    rv = Convert.ToDateTime(reader[columnName]);
                    return rv;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return null;
            }
        }

        public static long ReadLong(this DbDataReader reader, string columnName)
        {
            long? l = ReadNullableLong(reader, columnName);
            if (l.HasValue)
                return l.Value;
            else
                return default(long);
        }

        public static long? ReadNullableLong(this DbDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader[columnName] == DBNull.Value)
                return null;
            else
            {
                try
                {
                    long rv;
                    rv = Convert.ToInt64(reader[columnName]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


                return null;
            }
        }

        public static Guid ReadGuid(this DbDataReader reader, string columnName)
        {
            Guid? guid = ReadNullableGuid(reader, columnName);
            if (guid.HasValue)
                return guid.Value;
            else
                return Guid.Empty;
        }

        public static Guid? ReadNullableGuid(this DbDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader[columnName] == DBNull.Value)
                return null;
            else
            {
                Guid rv;
                try
                {
                    if (reader[columnName] is Guid)
                        rv = (Guid)reader[columnName];
                    else
                        rv = new Guid(reader[columnName].ToString());
                    return rv;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return null;

            }
        }

        public static bool ReadBool(this DbDataReader reader, string columnName)
        {
            bool? b = ReadNullableBool(reader, columnName);
            if (b.HasValue)
                return b.Value;
            else
                return false;
        }

        public static bool? ReadNullableBool(this DbDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader[columnName] == DBNull.Value)
                return null;
            else
            {
                bool rv = false;
                try
                {
                    if (reader[columnName] is bool)
                        rv = (bool)reader[columnName];
                    else if (reader[columnName] is int)
                    {
                        if ((int)reader[columnName] > 0)
                            rv = true;
                    }
                    return rv;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return null;

            }
        }

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

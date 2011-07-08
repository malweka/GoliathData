using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.DataAccess
{
    class TypeConverter
    {
        Dictionary<Type, Func<Object, Object>> converters = new Dictionary<Type, Func<Object, Object>>();

        void Load()
        {
            AddConverter(typeof(string), ReadString);
            //Int16
            AddConverter(typeof(Nullable<short>), ReadNullableInt16);
            AddConverter(typeof(short), ReadInt16);
            //Int32
            AddConverter(typeof(Nullable<int>), ReadNullableInt32);
            AddConverter(typeof(int), ReadInt32);
            //Int64
            AddConverter(typeof(Nullable<long>), ReadNullableInt64);
            AddConverter(typeof(int), ReadInt64);
            //Datetime
            AddConverter(typeof(Nullable<DateTime>), ReadNullableDateTime);
            AddConverter(typeof(DateTime), ReadDateTime);
        }

        internal void AddConverter(Type toType, Func<Object, Object> convertMethod)
        {
            if (convertMethod == null)
                throw new ArgumentNullException("convertMethod");
            if (toType == null)
                throw new ArgumentNullException("toType");

            if (converters.ContainsKey(toType))
                converters.Remove(toType);

            converters.Add(toType, convertMethod);
        }

        static object ReadString(object value)
        {
            if (value == DBNull.Value)
                return null;

            return value.ToString();
        }

        #region Convert Integers

        static object ReadNullableInt16(object value)
        {
            if (value == DBNull.Value)
                return null;

            if (value is short)
                return value;

            return Convert.ToInt16(value);
        }

        static object ReadInt16(object value)
        {
            var obj = ReadNullableInt16(value);
            if (obj == null)
                return default(short);
            else
                return obj;
        }

        static object ReadNullableInt32(object value)
        {
            if (value == DBNull.Value)
                return null;

            if (value is int)
                return value;

            return Convert.ToInt32(value);
        }

        static object ReadInt32(object value)
        {
            var obj = ReadNullableInt32(value);
            if (obj == null)
                return default(int);
            else
                return obj;
        }

        static object ReadNullableInt64(object value)
        {
            if (value == DBNull.Value)
                return null;

            if (value is long)
                return value;

            return Convert.ToInt64(value);
        }

        static object ReadInt64(object value)
        {
            var obj = ReadNullableInt64(value);
            if (obj == null)
                return default(short);
            else
                return obj;
        }

        #endregion

        #region Convert Dates and times

        static object ReadNullableDateTime(object value)
        {
            if (value == DBNull.Value)
                return null;

            if (value is DateTime)
                return value;

            if (value is long)
                return new DateTime((long)value);

            return Convert.ToDateTime(value);
        }

        static object ReadDateTime(object value)
        {
            var obj = ReadNullableDateTime(value);
            if (obj == null)
                return default(DateTime);
            else
                return obj;
        }

        //TODO: implement DateTimeOffset

        #endregion

        #region Convert Chars
        #endregion

        #region double precision
        #endregion

        #region binaries
        
        #endregion

        #region booleans
        #endregion
    }
}

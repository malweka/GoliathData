﻿using System;
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
            //char
            AddConverter(typeof(Nullable<char>), ReadNullableChar);
            AddConverter(typeof(char), ReadChar);
            //boolean
            AddConverter(typeof(Nullable<bool>), ReadNullableBoolean);
            AddConverter(typeof(bool), ReadBoolean);
            //Guids
            AddConverter(typeof(Nullable<Guid>), ReadNullableGuid);
            AddConverter(typeof(Guid), ReadGuid);
            //single
            AddConverter(typeof(Nullable<float>), ReadNullableSingle);
            AddConverter(typeof(float), ReadSingle);
            //double
            AddConverter(typeof(Nullable<double>), ReadNullableDouble);
            AddConverter(typeof(double), ReadDouble);
            //decimal
            AddConverter(typeof(Nullable<decimal>), ReadNullableDecimal);
            AddConverter(typeof(decimal), ReadDecimal);
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

        static object ReadNullableChar(object value)
        {
            if (value == DBNull.Value)
                return null;
            if (value is char)
                return value;
            return Convert.ToChar(value);
        }

        static object ReadChar(object value)
        {
            var obj = ReadNullableChar(value);
            if (obj == null)
                return default(char);
            else
                return obj;
        }

        #endregion

        #region double precision

        static object ReadNullableSingle(object value)
        {
            if (value == DBNull.Value)
                return null;
            if (value is float)
                return value;
            return Convert.ToSingle(value);
        }

        static object ReadSingle(object value)
        {
            var obj = ReadNullableSingle(value);
            if (obj == null)
                return default(float);
            else
                return obj;
        }

        static object ReadNullableDouble(object value)
        {
            if (value == DBNull.Value)
                return null;
            if (value is double)
                return value;
            return Convert.ToDouble(value);
        }
        static object ReadDouble(object value)
        {
            var obj = ReadNullableDouble(value);
            if (obj == null)
                return default(double);
            else
                return obj;
        }

        static object ReadNullableDecimal(object value)
        {
            if (value == DBNull.Value)
                return null;
            if (value is decimal)
                return value;
            return Convert.ToDecimal(value);
        }

        static object ReadDecimal(object value)
        {
            var obj = ReadNullableDecimal(value);
            if (obj == null)
                return default(decimal);
            else
                return obj;
        }

        #endregion

        #region binaries
        
        #endregion

        #region GUIDs

        static object ReadNullableGuid(object value)
        {
            if (value == DBNull.Value)
                return null;
            if (value is Guid)
                return value;
            if(value is string)
                return new Guid((string)value);
            if (value is byte[])
                return new Guid((byte[])value);

            throw new TypeConversionException(value.GetType(), typeof(Guid));
            
        }

        static object ReadGuid(object value)
        {
            var obj = ReadNullableGuid(value);
            if (obj == null)
                return Guid.Empty;
            else
                return obj;                
        }

        #endregion

        #region booleans

        static object ReadNullableBoolean(object value)
        {
            if (value == DBNull.Value)
                return null;
            if (value is bool)
                return value;

            if ((value is int) || (value is long) || (value is short))
            {
                long lv = (long)value;
                if (lv > 0)
                    return true;
                else
                    return false;
            }

            return Convert.ToBoolean(value);
        }

        static object ReadBoolean(object value)
        {
            var obj = ReadNullableBoolean(value);
            if (obj == null)
                return false;
            else
                return obj;
        }

        #endregion
    }
}

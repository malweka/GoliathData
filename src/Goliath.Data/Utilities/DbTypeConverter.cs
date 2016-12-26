using System;

namespace Goliath.Data.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class DbTypeConverter
    {
        /// <summary>
        /// Converts to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T ConvertTo<T>(object value)
        {
            var returnType = typeof(T);

            try
            {
                if ((value == null) || (value == DBNull.Value))
                    return default(T);

                var providedType = value.GetType();

                if (providedType == returnType)
                    return (T)value;

                else if (returnType == typeof(string))
                {
                    return (T)ConvertToString(value);
                }

                else if (returnType == typeof(bool) || returnType == typeof(bool?))
                {
                    return (T)ConvertToBoolean(value);
                }
                else if (returnType == typeof(byte) || returnType == typeof(byte?))
                {
                    return (T)ConvertToByte(value);
                }

                else if (returnType == typeof(int) || returnType == typeof(int?))
                {
                    return (T)ConvertToInt(value);
                }

                else if (returnType == typeof(short) || returnType == typeof(short?))
                {
                    return (T)ConvertToShort(value);
                }

                else if (returnType == typeof(long) || returnType == typeof(long?))
                {
                    return (T)ConvertToLong(value);
                }

                else if (returnType == typeof(Guid) || returnType == typeof(Guid?))
                {
                    return (T)ConvertToGuid(value);
                }

                else if (returnType == typeof(DateTime) || returnType == typeof(DateTime?))
                {
                    return (T)ConvertToDateTime(value);
                }

                else if (returnType == typeof(double) || returnType == typeof(double?))
                {
                    return (T)ConvertToDouble(value);
                }

                else if (returnType == typeof(float) || returnType == typeof(float?))
                {
                    return (T)ConvertToFloat(value);
                }

                else if (returnType == typeof(decimal) || returnType == typeof(decimal?))
                {
                    return (T)ConvertToDecimal(value);
                }

                else if (returnType.IsEnum)
                {
                    return (T)ConvertToEnum(returnType, value);
                }

                else
                {
                    throw new GoliathDataException($"Cannot convert {providedType.FullName} to {returnType.FullName}");
                }
            }
            catch (GoliathDataException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                throw new GoliathDataException($"Cannot convert to {returnType.FullName}", ex);
            }

        }


        static object ConvertToString(object value)
        {
            return value.ToString();
        }

        static object ConvertToInt(object value)
        {
            return Convert.ToInt32(value);
        }

        static object ConvertToGuid(object value)
        {
            if (value is Guid)
                return value;
            if (value is string)
                return new Guid(value.ToString());
            if (value is byte[])
                return new Guid((byte[])value);

            throw new GoliathDataException($"Cannot convert {value.GetType()} type Guid.");
        }

        static object ConvertToShort(object value)
        {
            return Convert.ToInt16(value);
        }

        static object ConvertToLong(object value)
        {
            return Convert.ToInt64(value);
        }

        static object ConvertToDateTime(object value)
        {
            return Convert.ToDateTime(value);
        }

        static object ConvertToDouble(object value)
        {
            return Convert.ToDouble(value);
        }

        static object ConvertToFloat(object value)
        {
            return Convert.ToSingle(value);
        }

        static object ConvertToDecimal(object value)
        {
            return Convert.ToDecimal(value);
        }

        static object ConvertToByte(object value)
        {
            return Convert.ToByte(value);
        }

        static object ConvertToBoolean(object value)
        {
            if ("0".Equals(value)) return false;
            else if ("1".Equals(value)) return true;
            return Convert.ToBoolean(value);
        }

        static object ConvertToEnum(Type enumType, object value)
        {
            if (value is string)
            {
                return Enum.Parse(enumType, value.ToString(), true);
            }
            else
            {
                var obj = Enum.ToObject(enumType, value);
                return obj;
            }
        }

    }
}
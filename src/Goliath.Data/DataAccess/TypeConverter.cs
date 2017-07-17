using System;
using System.Collections.Generic;
using Goliath.Data.Utilities;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TypeConverterStore : ITypeConverterStore
    {
        readonly Dictionary<Type, Func<object, object>> converters = new Dictionary<Type, Func<object, object>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConverterStore"/> class.
        /// </summary>
        public TypeConverterStore()
        {
            Load();
        }

        void Load()
        {
            AddConverter(typeof(string), DbTypeConverter.ConvertTo<string>);
            //byte
            AddConverter(typeof(byte?), (x) => DbTypeConverter.ConvertTo<byte?>(x));
            AddConverter(typeof(byte), (x) => DbTypeConverter.ConvertTo<byte>(x));
            //Int16
            AddConverter(typeof(short?), (x)=>DbTypeConverter.ConvertTo<short?>(x));
            AddConverter(typeof(short), (x) => DbTypeConverter.ConvertTo<short>(x));
            //Int32
            AddConverter(typeof(int?), (x) => DbTypeConverter.ConvertTo<int?>(x));
            AddConverter(typeof(int), (x) => DbTypeConverter.ConvertTo<int>(x));
            //Int64
            AddConverter(typeof(long?), (x) => DbTypeConverter.ConvertTo<long?>(x));
            AddConverter(typeof(long), (x) => DbTypeConverter.ConvertTo<long>(x));
            //Datetime
            AddConverter(typeof(DateTime?), (x) => DbTypeConverter.ConvertTo<DateTime?>(x));
            AddConverter(typeof(DateTime), (x) => DbTypeConverter.ConvertTo<DateTime>(x));
            //DatetimeOffset
            AddConverter(typeof(DateTimeOffset?), (x) => DbTypeConverter.ConvertTo<DateTimeOffset?>(x));
            AddConverter(typeof(DateTimeOffset), (x) => DbTypeConverter.ConvertTo<DateTimeOffset>(x));
            //char
            AddConverter(typeof(char?), (x) => DbTypeConverter.ConvertTo<char?>(x));
            AddConverter(typeof(char), (x) => DbTypeConverter.ConvertTo<char>(x));
            //boolean
            AddConverter(typeof(bool?), (x) => DbTypeConverter.ConvertTo<bool?>(x));
            AddConverter(typeof(bool), (x) => DbTypeConverter.ConvertTo<bool>(x));
            //Guids
            AddConverter(typeof(Guid?), (x) => DbTypeConverter.ConvertTo<Guid?>(x));
            AddConverter(typeof(Guid), (x) => DbTypeConverter.ConvertTo<Guid>(x));
            //single
            AddConverter(typeof(float?), (x) => DbTypeConverter.ConvertTo<float?>(x));
            AddConverter(typeof(float), (x) => DbTypeConverter.ConvertTo<float>(x));
            //double
            AddConverter(typeof(double?), (x) => DbTypeConverter.ConvertTo<double?>(x));
            AddConverter(typeof(double), (x) => DbTypeConverter.ConvertTo<double>(x));
            //decimal
            AddConverter(typeof(decimal?), (x) => DbTypeConverter.ConvertTo<decimal?>(x));
            AddConverter(typeof(decimal), (x) => DbTypeConverter.ConvertTo<decimal>(x));
        }

        /// <summary>
        /// Adds the converter.
        /// </summary>
        /// <param name="toType">To type.</param>
        /// <param name="convertMethod">The convert method.</param>
        public void AddConverter(Type toType, Func<object, object> convertMethod)
        {
            if (convertMethod == null)
                throw new ArgumentNullException(nameof(convertMethod));

            if (toType == null)
                throw new ArgumentNullException(nameof(toType));

            if (converters.ContainsKey(toType))
                converters.Remove(toType);

            converters.Add(toType, convertMethod);
        }

        /// <summary>
        /// Gets the converter factory method for the specified type.
        /// </summary>
        /// <param name="typeToConverTo">The type to conver to.</param>
        /// <returns></returns>
        /// <exception cref="Goliath.Data.DataAccessException"></exception>
        public Func<Object, Object> GetConverterFactoryMethod(Type typeToConverTo)
        {
            if (typeToConverTo == null)
                throw new ArgumentNullException(nameof(typeToConverTo));

            Func<Object, Object> converter;
            if (converters.TryGetValue(typeToConverTo, out converter))
            {
                return converter;
            }
            else
                throw new DataAccessException("No converter for type {0} was found", typeToConverTo);
        }

        /// <summary>
        /// Converts to enum.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public object ConvertToEnum(Type enumType, object value)
        {
            return ConvertValueToEnum(enumType, value);
        }

        /// <summary>
        /// Converts the value to enum.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static object ConvertValueToEnum(Type enumType, object value)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            if (value == DBNull.Value || value == null)
                return null;

            Object enumVal = null;

            if (value is string)
            {
                enumVal = Enum.Parse(enumType, value.ToString(), true);
            }
            else if ((value is Int16) || (value is Int32) || (value is Int64))
            {
                enumVal = Enum.ToObject(enumType, value);
            }
            else
            {
                throw new TypeConversionException(value.GetType(), enumType);
            }

            return enumVal;
        }

        
    }
}

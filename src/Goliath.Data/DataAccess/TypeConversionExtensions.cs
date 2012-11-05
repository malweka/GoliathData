using System;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Utils;

namespace Goliath.Data.DataAccess
{
    internal static class TypeConversionExtensions
    {
        /// <summary>
        /// Determines whether this instance [can generate key] the specified pk.
        /// </summary>
        /// <param name="pk">The pk.</param>
        /// <param name="pInfo">The p info.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="typeConverterStore">The type converter store.</param>
        /// <returns>
        /// 	<c>true</c> if this instance [can generate key] the specified pk; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanGenerateKey(this PrimaryKeyProperty pk, PropertyAccessor pInfo, object entity,
                                          ITypeConverterStore typeConverterStore)
        {
            if (pk.KeyGenerator == null)
                return false;

            //now let's  check the value
            var idValue = pInfo.GetMethod(entity);

            if (idValue == null)
                return true;

            var converter = typeConverterStore.GetConverterFactoryMethod(idValue.GetType());

            try
            {
                object unsavedVal = converter.Invoke(pk.UnsavedValueString);
                if (idValue.Equals(unsavedVal))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                throw new MappingException(
                    string.Format("Could not convert '{0}' to {1} for property {2}", pk.UnsavedValueString,
                                  idValue.GetType().FullName, pk.Key.PropertyName), ex);
            }

        }

        public static object GetUnsavedValue(this PrimaryKeyProperty pk, Type type, ITypeConverterStore typeConverterStore)
        {
            if (pk == null)
                return null;


            if (string.IsNullOrWhiteSpace(pk.UnsavedValueString))
            {
                if (type.IsValueType && type.IsPrimitive)
                {
                    if (type == typeof(string))
                        return string.Empty;

                    if ((type == typeof(int)) || (type == typeof(int?)) || (type == typeof(long)) ||
                        (type == typeof(long?)) || (type == typeof(short)) || (type == typeof(short?)))
                        return 0;

                    if ((type == typeof(double)) || (type == typeof(double?)) || (type == typeof(float)) ||
                        (type == typeof(float?)))
                        return 0.0;

                    if ((type == typeof(Guid)) || (type == typeof(Guid?)))
                        return Guid.Empty;
                }
            }
            else
            {
                var convertMethod = typeConverterStore.GetConverterFactoryMethod(type);
                var value = convertMethod(pk.UnsavedValueString);
                return value;
            }

            return null;
        }
    }
}
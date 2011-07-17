using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Adds the converter.
        /// </summary>
        /// <param name="toType">To type.</param>
        /// <param name="convertMethod">The convert method.</param>
        void AddConverter(Type toType, Func<Object, Object> convertMethod);
        /// <summary>
        /// Gets the converter.
        /// </summary>
        /// <param name="from">From.</param>
        /// <returns></returns>
        Func<Object, Object> GetConverter(Type from);
        /// <summary>
        /// Converts to enum.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        object ConvertToEnum(Type enumType, object value);
    }
}

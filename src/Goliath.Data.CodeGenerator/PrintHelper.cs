using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data;
using Goliath.Data.Mapping;

namespace Goliath.Data.CodeGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public static class PrintHelper
    {
        /// <summary>
        /// Prints the description.
        /// </summary>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        public static string PrintDescription(this EntityMap entityMap)
        {
            if(entityMap != null)
            {
                string print;
                if (entityMap.MetaDataAttributes.TryGetValue("description", out print))
                    return print;
            }

            return string.Empty;
        }

        /// <summary>
        /// Prints the description.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public static string PrintDescription(this Property property)
        {
            if (property != null)
            {
                string print;
                if (property.MetaDataAttributes.TryGetValue("description", out print))
                    return print;
            }

            return string.Empty;
        }
    }
}

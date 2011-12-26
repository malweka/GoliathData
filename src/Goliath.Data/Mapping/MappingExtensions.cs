using System;
using System.Xml;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// Mapping Extensions methods
    /// </summary>
    public static class MappingExtensions
    {
        //static bool ComplexTypeContainsProperty(string propertyName, 
        /// <summary>
        /// Determines whether this instance can print the specified property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can print the specified property; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanPrint(this Property property, EntityMap entity)
        {
            if (entity == null)
                return true;
            if (string.IsNullOrWhiteSpace(entity.Extends))
                return true;

            var baseModel = entity.BaseModel;

            if (baseModel == null)
                return true;

            try
            {
                if (baseModel is ComplexType)
                {
                    var complexType = baseModel as ComplexType;

                    if (complexType.Properties.Contains(property.Name))
                        return false;
                }
                else if (baseModel is EntityMap)
                {
                    var ent = baseModel as EntityMap;

                    if (ent.IsSubClass)
                    {
                        var supEnt = ent.Parent.GetEntityMap(ent.Extends);
                        return CanPrint(property, supEnt);
                    }
                    else if ((ent.BaseModel != null) && (ent.BaseModel is ComplexType))
                    {
                        var supComplex = ent.BaseModel as ComplexType;
                        if (supComplex.Properties.Contains(property.Name))
                            return false;
                    }

                    if (ent.Properties.Contains(property.Name))
                        return false;
                }
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        internal static bool CanReadElement(this XmlReader reader, string elementName)
        {
            if ((reader.NodeType == XmlNodeType.Element) && reader.Name.Equals(elementName))
            {
                if (reader.IsEmptyElement)
                {
                    if (reader.HasAttributes)
                        return true;
                    else
                        return false;
                }

                return true;
            }
            return false;
        }

        internal static bool HasReachedEndOfElement(this XmlReader reader, string elementName)
        {
            if ((reader.NodeType == XmlNodeType.EndElement) && reader.Name.Equals(elementName))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Truncates the over limit.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        [Obsolete]
        public static string TruncateOverLimit(this string val, int limit)
        {
            if (val.Length > limit)
            {
                string result = string.Format("{0}{1}", val.Substring(0, (limit - 9)), Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8));
                return result;
            }
            else
                return val;
        }

    }
}

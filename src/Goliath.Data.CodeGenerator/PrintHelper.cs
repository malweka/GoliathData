using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

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
            if (entityMap != null)
            {
                string print;
                if (entityMap.MetaDataAttributes.TryGetValue("description", out print))
                    return print;
                else
                    return string.Format("Class {0}", entityMap.Name.Titleize());
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

                else
                    return property.Name.Titleize();
            }

            return string.Empty;
        }

        public static string PrintPropertyDescriptionAttribute(this Property property, EntityMap entity, string resourceType)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            var sb = new StringBuilder();
            var displaySb = new StringBuilder();

            displaySb.AppendFormat("Display(Name = \"{0}\", Description = \"{1}\", ResourceType = typeof({2})", PrintResourceName(property, entity, ResourceItemType.Label),
                PrintResourceName(property, entity, ResourceItemType.Description), resourceType);

            if(property.MetaDataAttributes.Count > 0)
            {
                string displayPrompt;
                if (property.MetaDataAttributes.TryGetValue("display_prompt", out displayPrompt))
                    displaySb.AppendFormat(", Prompt = \"{0}\"", PrintResourceName(property, entity, ResourceItemType.Prompt));

                string displayOrder;
                if (property.MetaDataAttributes.TryGetValue("display_order", out displayOrder))
                    displaySb.AppendFormat(", Order = \"{0}\"", displayOrder);

                string displayGroupName;
                if (property.MetaDataAttributes.TryGetValue("display_groupname", out displayGroupName))
                    displaySb.AppendFormat(", GroupName = \"{0}\"", displayGroupName);

                displaySb.Append(")");
                sb.AppendFormat("[{0}]", displaySb.ToString());

                string editable;
                if (property.MetaDataAttributes.TryGetValue("editable", out editable))
                    sb.AppendFormat("\r\n\t\t[Editable({0})]", editable);

                string required;
                if (property.MetaDataAttributes.TryGetValue("required", out required))
                    sb.AppendFormat("\r\n\t\t[Required(ErrorMessageResourceName = \"{0}\", ErrorMessageResourceType = typeof({1}))]", PrintResourceName(property, entity, ResourceItemType.ErrorWhenMissing), resourceType);

            }
            else
            {
                displaySb.Append(")");
                sb.AppendFormat("[{0}]", displaySb.ToString());
            }
           
            return sb.ToString();
        }

        /// <summary>
        /// Tries the get attribute.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="key">The key.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static bool TryGetAttribute(this Property property, string key, out string attributeValue)
        {
            if (key == null) throw new ArgumentNullException("key");
            return property.MetaDataAttributes.TryGetValue(key, out attributeValue);
        }


        /// <summary>
        /// Prints the display description.
        /// </summary>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        public static string PrintDisplayDescription(this EntityMap entityMap)
        {
            if (entityMap != null)
            {
                string print;
                if (entityMap.MetaDataAttributes.TryGetValue("display_description", out print))
                    return print;
            }

            return PrintDescription(entityMap);
        }

        /// <summary>
        /// Prints the display description.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public static string PrintDisplayDescription(this Property property)
        {
            if (property != null)
            {
                string print;
                if (property.MetaDataAttributes.TryGetValue("display_description", out print))
                    return print;

                //else if (property.MetaDataAttributes.TryGetValue("description", out print))
                //    return print;
            }

            return PrintDescription(property);
        }

        public static string PrintResourceName(this EntityMap entity, ResourceItemType resourceType)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            if (resourceType == ResourceItemType.Description)
                return entity.FullName.Replace(".", "_").ToLower() + "_description";
            else
                return entity.FullName.Replace(".", "_").ToLower() + "_label";
        }

        public static string PrintClassResourceName(string classFullName, ResourceItemType resourceType)
        {
            if (classFullName == null) throw new ArgumentNullException("classFullName");

            if (resourceType == ResourceItemType.Description)
                return classFullName.Replace(".", "_").ToLower() + "_description";
            else
                return classFullName.Replace(".", "_").ToLower() + "_label";
        }

        /// <summary>
        /// Prints the name of the property group resource.
        /// </summary>
        /// <param name="propertyGroupName">Name of the property group.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        public static string PrintPropertyGroupResourceName(string propertyGroupName, ResourceItemType resourceType)
        {
            if (propertyGroupName == null) throw new ArgumentNullException("propertyGroupName");

            if (resourceType == ResourceItemType.Description)
                return string.Format("prop_group_{0}_description", propertyGroupName);
            else
                return string.Format("prop_group_{0}_label", propertyGroupName);
        }

        /// <summary>
        /// Prints the name of the resource.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        public static string PrintResourceName(this Property prop, EntityMap entity, ResourceItemType resourceType)
        {
            if (prop == null) throw new ArgumentNullException("prop");
            if (entity == null) throw new ArgumentNullException("entity");

            string rname = string.Format("{0}_{1}", entity.FullName.Replace(".", "_").ToLower(), prop.Name.ToLower());
            switch (resourceType)
            {
                case ResourceItemType.Description:
                    return rname + "_description";
                case ResourceItemType.ErrorWhenMissing:
                    return rname + "_reqError";
                case ResourceItemType.Prompt:
                    return rname + "_dispPrompt";
                default:
                    return rname + "_label";
            }
        }

        public static string PrintPropertyResourceName(string propertyName, string classFullName, ResourceItemType resourceType)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            if (classFullName == null) throw new ArgumentNullException("classFullName");

            string rname = string.Format("{0}_{1}", classFullName.Replace(".", "_").ToLower(), propertyName.ToLower());
            switch (resourceType)
            {
                case ResourceItemType.Description:
                    return rname + "_description";
                case ResourceItemType.ErrorWhenMissing:
                    return rname + "_reqError";
                case ResourceItemType.Prompt:
                    return rname + "_dispPrompt";
                default:
                    return rname + "_label";
            }
        }
    }

    public enum ResourceItemType
    {
        Label,
        Description,
        Prompt,
        ErrorWhenMissing,
    }
}

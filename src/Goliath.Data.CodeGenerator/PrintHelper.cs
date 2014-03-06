using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
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

        /// <summary>
        /// Prints the property description attribute.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">entity</exception>
        public static string PrintPropertyDescriptionAttribute(this Property property, EntityMap entity, string resourceType)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            var sb = new StringBuilder();
            var displaySb = new StringBuilder();

            displaySb.AppendFormat("Display(Name = \"{0}\", Description = \"{1}\", ResourceType = typeof({2})", PrintResourceName(property, entity, ResourceItemType.Label),
                PrintResourceName(property, entity, ResourceItemType.Description), resourceType);

            if (property.MetaDataAttributes.Count > 0)
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

        /// <summary>
        /// Prints the name of the resource.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">entity</exception>
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

        static string GetResourceName(string rname, ResourceItemType resourceType)
        {
            switch (resourceType)
            {
                case ResourceItemType.Description:
                    return rname + "_description";
                case ResourceItemType.ErrorWhenMissing:
                    return rname + "_reqError";
                case ResourceItemType.ErrorNull:
                    return string.Concat(rname, "_nullError");
                case ResourceItemType.ErrorLength:
                    return string.Concat(rname, "_lengthError");
                case ResourceItemType.Prompt:
                    return rname + "_dispPrompt";
                default:
                    return rname + "_label";
            }
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

            var rname = string.Format("{0}_{1}", entity.FullName.Replace(".", "_").ToLower(), prop.Name.ToLower());
            return GetResourceName(rname, resourceType);
        }

        /// <summary>
        /// Prints the name of the property resource.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="classFullName">Full name of the class.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// propertyName
        /// or
        /// classFullName
        /// </exception>
        public static string PrintPropertyResourceName(string propertyName, string classFullName, ResourceItemType resourceType)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            if (classFullName == null) throw new ArgumentNullException("classFullName");

            var rname = string.Format("{0}_{1}", classFullName.Replace(".", "_").ToLower(), propertyName.ToLower());
            return GetResourceName(rname, resourceType);
        }

        /// <summary>
        /// Gets the name of the class.
        /// </summary>
        /// <param name="classFullName">Full name of the class.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">classFullName</exception>
        public static string GetClassName(string classFullName)
        {
            if (classFullName == null) throw new ArgumentNullException("classFullName");
            var xname = classFullName.Substring(classFullName.LastIndexOf(".") + 1);
            return xname;
        }

        /// <summary>
        /// Prints the statement parameters.
        /// </summary>
        /// <param name="dialect">The dialect.</param>
        /// <param name="stat">The stat.</param>
        /// <returns></returns>
        public static string PrintStatementParameters(SqlDialect dialect, StatementMap stat)
        {
            var parameters = new List<string>();

            if (stat.InputParametersMap.Count > 0)
            {
                foreach (var inputParam in stat.InputParametersMap)
                {
                    parameters.Add(string.Format("{0} {1}", inputParam.Value, inputParam.Key));
                }
            }
            else if (stat.DbParametersMap.Count > 0)
            {
                foreach (var dbParam in stat.DbParametersMap)
                {
                    string dbParamType;
                    if (dbParam.Value == null)
                        dbParamType = "object";
                    else
                        dbParamType = SqlDialect.PrintClrTypeToString(dialect.GetClrType(dbParam.Value.Value, true), false);
                    parameters.Add(string.Format("{0} {1}", dbParamType, dbParam.Key));
                }
            }

            return string.Join(", ", parameters);
        }

        /// <summary>
        /// Prints the statement parameter names.
        /// </summary>
        /// <param name="dialect">The dialect.</param>
        /// <param name="stat">The stat.</param>
        /// <returns></returns>
        public static string PrintStatementParameterNames(SqlDialect dialect, StatementMap stat)
        {
            var parameters = new List<string>();

            if (stat.InputParametersMap.Count > 0)
            {
                foreach (var inputParam in stat.InputParametersMap)
                {
                    parameters.Add(inputParam.Key);
                }
            }
            else if (stat.DbParametersMap.Count > 0)
            {
                foreach (var dbParam in stat.DbParametersMap)
                {
                    parameters.Add(dbParam.Key);
                }
            }

            return string.Join(", ", parameters);
        }

        /// <summary>
        /// Prints the statement query parameters.
        /// </summary>
        /// <param name="dialect">The dialect.</param>
        /// <param name="stat">The stat.</param>
        /// <returns></returns>
        public static string PrintStatementQueryParams(SqlDialect dialect, StatementMap stat)
        {
            var parameters = new List<string>();

            if (stat.InputParametersMap.Count > 0)
            {
                foreach (var inputParam in stat.InputParametersMap)
                {
                    parameters.Add(string.Format("new QueryParam(\"{0}\", {0})", inputParam.Key));
                }
            }
            else if (stat.DbParametersMap.Count > 0)
            {
                foreach (var dbParam in stat.DbParametersMap)
                {
                    parameters.Add(string.Format("new QueryParam(\"{0}\", {0})", dbParam.Key));
                }
            }

            return string.Join(", ", parameters);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ResourceItemType
    {
        /// <summary>
        /// The label
        /// </summary>
        Label,
        /// <summary>
        /// The description
        /// </summary>
        Description,
        /// <summary>
        /// The prompt
        /// </summary>
        Prompt,
        /// <summary>
        /// The error when missing
        /// </summary>
        ErrorWhenMissing,
        /// <summary>
        /// The error null
        /// </summary>
        ErrorNull,
        /// <summary>
        /// The error length
        /// </summary>
        ErrorLength,
    }
}

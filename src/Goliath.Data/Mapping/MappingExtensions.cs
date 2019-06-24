using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// Mapping Extensions methods
    /// </summary>
    public static class MappingExtensions
    {
        static ILogger logger;
        static MappingExtensions()
        {
            logger = Logger.GetLogger(typeof(MappingExtensions));
        }

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

            if (property.MetaDataAttributes.TryGetValue("printable", out string val))
            {
                if (val.ToUpper() == "FALSE")
                    return false;
            }

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
                logger.Log(LogLevel.Debug, string.Format("CanPrint:{0}.{1}\n\n{2}", entity.Name, property.PropertyName, ex.ToString()));
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

        internal static string PrintUnsavedValue(this PrimaryKeyProperty property)
        {
            string rVal = null;
            if(!string.IsNullOrWhiteSpace(property.UnsavedValueString))
            {
                var dbType = property.Key.DbType;
                switch (dbType)
                {
                    case DbType.Decimal:
                    case DbType.Double:
                    case DbType.Int16:
                    case DbType.Int32:
                    case DbType.Int64:
                    case DbType.VarNumeric:
                    case DbType.UInt64:
                    case DbType.UInt32:
                    case DbType.UInt16:
                        rVal = property.UnsavedValueString;
                        break;
                    case DbType.Guid:
                        rVal = string.Format("new Guid(\"{0}\")", property.UnsavedValueString);
                        break;
                    default:
                        rVal = string.Format("\"{0}\"", property.UnsavedValueString);
                        break;
                }
            }

            return rVal;
        }

        internal static bool HasReachedEndOfElement(this XmlReader reader, string elementName)
        {
            if ((reader.NodeType == XmlNodeType.EndElement) && reader.Name.Equals(elementName))
            {
                return true;
            }
            return false;
        }

        public static bool IsMappingComplexType(this Property property)
        {
            if (property.MetaDataAttributes.TryGetValue("complexType", out string val))
            {
                if (val.ToUpper() == "TRUE")
                    return true;
            }

            return false;
        }

        public static bool IsMarkedPrintable(this Property property)
        {
            if (property.MetaDataAttributes.TryGetValue("printable", out string val))
            {
                if (val.ToUpper() == "FALSE")
                    return false;
            }

            return true;
        }

        public static ICollection<Property> GetPropertiesNotInComplexType(this ComplexType complexType, EntityMap entity)
        {
            var props = complexType.Properties.Except(entity);
            return props.ToList();
        }

        public static ICollection<Property> GetPropertiesInCommonWithComplexType(this ComplexType complexType, EntityMap entity)
        {
            var props = complexType.Properties.Intersect(entity);
            return props.ToList();
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


        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">entityMap</exception>
        public static DataTable CreateTable(this EntityMap entityMap)
        {
            if (entityMap == null) throw new ArgumentNullException(nameof(entityMap));

            var table = new DataTable(entityMap.TableName);
            if (entityMap.PrimaryKey != null)
            {
                foreach (Property key in entityMap.PrimaryKey.Keys)
                {
                    var column = new DataColumn(key.ColumnName, SqlTypeHelper.GetClrType(key.DbType,false)){ AllowDBNull = key.IsNullable };
                    table.Columns.Add(column);
                }
            }

            foreach (var prop in entityMap.Properties)
            {
                if(table.Columns.Contains(prop.ColumnName) || prop.IsMappingComplexType())
                    continue;
                
                var column = new DataColumn(prop.ColumnName, SqlTypeHelper.GetClrType(prop.DbType, false)){ AllowDBNull = prop.IsNullable };
                table.Columns.Add(column);
            }

            foreach (var rel in entityMap.Relations)
            {
                if (rel.RelationType > RelationshipType.ManyToOne)
                    continue;

                if (table.Columns.Contains(rel.ColumnName))
                    continue; //column has already been added 

                DataColumn column = new DataColumn(rel.ColumnName, SqlTypeHelper.GetClrType(rel.DbType, false)){ AllowDBNull = rel.IsNullable };
                table.Columns.Add(column);
            }

            return table;
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="datababag">The datababag.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">datababag</exception>
        public static DataTable CreateTable(this EntityMap entityMap, IList<IDictionary<string, object>> datababag)
        {
            var table = CreateTable(entityMap);
            if (datababag == null) throw new ArgumentNullException(nameof(datababag));

            foreach (var row in datababag)
            {
                var dataRow = table.NewRow();
                foreach (var keypair in row)
                {
                    if(!table.Columns.Contains(keypair.Key))
                        continue;

                    dataRow[keypair.Key] = keypair.Value ?? DBNull.Value;
                }

                table.Rows.Add(dataRow);
                dataRow.AcceptChanges();
            }

            return table;
        }

    }
}

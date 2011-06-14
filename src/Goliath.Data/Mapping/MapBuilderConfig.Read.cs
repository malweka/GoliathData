using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MapConfig
    {
        class MapReader
        {
            public void Read(XmlReader reader, MapConfig config)
            {
                while (reader.ReadToFollowing("goliath.data"))
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            ProcessElement(reader, config);
                            break;
                        default:
                            break;
                    }
                }
            }

            #region Type conversion methods

            bool ReadBool(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;
                bool retVal;
                if (bool.TryParse(value, out retVal))
                    return retVal;

                throw new MappingSerializationException(string.Format("could not convert {0} to boolean.", value),
                        new Exception());

            }

            int ReadInteger(string intValue)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(intValue))
                        return 0;
                    int result = Convert.ToInt32(intValue);
                    return result;
                }
                catch (Exception ex)
                {
                    throw new MappingSerializationException(string.Format("could not convert {0} to integer.", intValue),
                        ex);
                }
            }

            Type ReadClrType(string typeNameAsString)
            {
                if (string.IsNullOrWhiteSpace(typeNameAsString))
                    return null;
                try
                {
                    Type type = Type.GetType(typeNameAsString, true, true);
                    return type;
                }
                catch (Exception ex)
                {
                    throw new MappingSerializationException(string.Format("type {0} not found. Are you missing assembly reference?", typeNameAsString),
                        ex);
                }
            }

            T ReadEnumType<T>(string constraintValue)
            {
                if (string.IsNullOrWhiteSpace(constraintValue))
                {
                    return default(T);
                }

                try
                {
                    var result = Enum.Parse(typeof(T), constraintValue, true);
                    return (T)result;

                }
                catch (Exception ex)
                {
                    throw new MappingSerializationException(string.Format("could not convert {0} to {1}.", constraintValue, typeof(T)),
                        ex);
                }
            }

            #endregion

            #region read methods

            void InitializeRelObject(ref Relation rel, Property prop)
            {
                if (rel == null)
                    rel = new Relation(prop);
            }

            Property Read_PropertyElement(XmlReader reader, MapConfig config, string entityName, bool forceReturnRel = false)
            {
                if (reader.CanReadElement(entityName))
                {
                    Property property = new Property();
                    Relation rel = null;

                    while (reader.MoveToNextAttribute())
                    {
                        string currentAttribute = reader.Name;
                        switch (currentAttribute)
                        {
                            case "name":
                                property.PropertyName = reader.Value;
                                break;
                            case "column":
                                property.ColumnName = reader.Value;
                                break;
                            case "constraintType":
                                property.ConstraintType = ReadEnumType<ConstraintType>(reader.Value);
                                break;
                            case "clrType":
                                property.ClrType = ReadClrType(reader.Value);
                                break;
                            case "dbType":
                                property.DbType = ReadEnumType<System.Data.DbType>(reader.Value);
                                break;
                            case "sqlType":
                                property.SqlType = reader.Value;
                                break;
                            case "length":
                                property.Length = ReadInteger(reader.Value);
                                break;
                            case "precision":
                                property.Precision = ReadInteger(reader.Value);
                                break;
                            case "scale":
                                property.Scale = ReadInteger(reader.Value);
                                break;
                            case "type":
                                property.ComplexTypeName = reader.Value;
                                break;
                            case "lazy":
                                property.LazyLoad = ReadBool(reader.Value);
                                break;
                            case "unique":
                                property.IsUnique = ReadBool(reader.Value);
                                break;
                            case "primaryKey":
                                property.IsPrimaryKey = ReadBool(reader.Value);
                                break;
                            case "default":
                                property.DefaultValue = reader.Value;
                                break;
                            case "constraint":
                                property.ConstraintName = reader.Value;
                                break;
                            case "ignoreOnUpdate":
                                property.IgnoreOnUpdate = ReadBool(reader.Value);
                                break;
                            case "nullable":
                                property.IsNullable = ReadBool(reader.Value);
                                break;
                            case "identity":
                                property.IsIdentity = ReadBool(reader.Value);
                                break;
                            case "autoGenerated":
                                property.IsAutoGenerated = ReadBool(reader.Value);
                                break;
                            //relation attributes
                            case "relation":
                                InitializeRelObject(ref rel, property);
                                rel.RelationType = ReadEnumType<RelationshipType>(reader.Value);
                                break;
                            case "referenceTable":
                                InitializeRelObject(ref rel, property);
                                rel.ReferenceTable = reader.Value;
                                break;
                            case "referenceColumn":
                                InitializeRelObject(ref rel, property);
                                rel.ReferenceColumn = reader.Value;
                                break;
                            case "keyFieldName":
                                InitializeRelObject(ref rel, property);
                                rel.KeyFieldName = reader.Value;
                                break;
                            case "refConstraint":
                                InitializeRelObject(ref rel, property);
                                rel.ReferenceConstraintName = reader.Value;
                                break;
                            case "refEntity":
                                InitializeRelObject(ref rel, property);
                                rel.ReferenceEntityName = reader.Value;
                                break;
                            case "exclude":
                                InitializeRelObject(ref rel, property);
                                rel.Exclude = ReadBool(reader.Value);
                                break;
                            default:
                                break;
                        }
                    }

                    if (rel != null)
                        return rel;

                    if (forceReturnRel)
                    {
                        rel = new Relation(property);
                        return rel;
                    }

                    return property;
                }
                return null;
            }

            ComplexType Read_ComplexTypeElement(XmlReader reader, MapConfig config)
            {
                ComplexType compType = new ComplexType(null);
                if (reader.CanReadElement("type"))
                {
                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.Name)
                        {
                            case "fullname":
                                compType.FullName = reader.Value;
                                break;
                            case "enum":
                                compType.IsEnum = ReadBool(reader.Value);
                                break;
                            default:
                                break;
                        }
                    }

                    while (reader.Read())
                    {
                        if (reader.HasReachedEndOfElement("type"))
                            return compType;

                        else if (reader.CanReadElement("properties"))
                        {
                            bool hasReachedEndGracefully = false;
                            while (reader.Read())
                            {
                                if (reader.CanReadElement("property"))
                                {
                                    var prop = Read_PropertyElement(reader, config, "property");
                                    if (prop != null)
                                        compType.Properties.Add(prop);

                                }
                                else if (reader.HasReachedEndOfElement("properties"))
                                {
                                    hasReachedEndGracefully = true;
                                    break;
                                }
                            }

                            if (!hasReachedEndGracefully)
                                throw new MappingSerializationException(typeof(PropertyCollection), "missing a </properties> end tag");
                        }
                    }

                    throw new MappingSerializationException(typeof(ComplexType), "missing a </type> end tag");

                }
                return null;
            }

            EntityMap Read_EntityElement(XmlReader reader, MapConfig config)
            {
                EntityMap entMap = new EntityMap();
                if (reader.CanReadElement("entity"))
                {
                    while (reader.MoveToNextAttribute())
                    {
                        string currentAttribute = reader.Name;
                        switch (currentAttribute)
                        {
                            case "name":
                                entMap.Name = reader.Value;
                                break;
                            case "extends":
                                entMap.Extends = reader.Value;
                                break;
                            case "assembly":
                                entMap.AssemblyName = reader.Value;
                                break;
                            case "entityNamespace":
                                entMap.Namespace = reader.Value;
                                break;
                            case "table":
                                entMap.TableName = reader.Value;
                                break;
                            case "schema":
                                entMap.SchemaName = reader.Value;
                                break;
                            case "tableAbbr":
                                entMap.TableAbbreviation = reader.Value;
                                break;
                            case "linkTable":
                                bool isLinkTable;
                                if (bool.TryParse(reader.Value, out isLinkTable))
                                    entMap.IsLinkTable = isLinkTable;
                                break;
                            default:
                                break;
                        }
                    }

                    while (reader.Read())
                    {
                        if (reader.HasReachedEndOfElement("entity"))
                            return entMap;

                        else if (reader.CanReadElement("primaryKey"))
                        {
                            bool hasReachedEndGracefully = false;
                            List<Property> keys = new List<Property>();
                            while (reader.Read())
                            {
                                if (reader.CanReadElement("key"))
                                {
                                    var prop = Read_PropertyElement(reader, config, "key");
                                    if (prop != null)
                                        keys.Add(prop);

                                }
                                else if (reader.HasReachedEndOfElement("primaryKey"))
                                {
                                    hasReachedEndGracefully = true;
                                    break;
                                }
                            }

                            if (!hasReachedEndGracefully)
                                throw new MappingSerializationException(typeof(PrimaryKey), "missing a </primaryKey> end tag");
                            entMap.PrimaryKey = new PrimaryKey(keys);
                        }

                        else if (reader.CanReadElement("properties"))
                        {
                            bool hasReachedEndGracefully = false;
                            while (reader.Read())
                            {
                                if (reader.CanReadElement("property"))
                                {
                                    var prop = Read_PropertyElement(reader, config, "property");
                                    if (prop != null)
                                        entMap.Properties.Add(prop);

                                }
                                else if (reader.HasReachedEndOfElement("properties"))
                                {
                                    hasReachedEndGracefully = true;
                                    break;
                                }
                            }

                            if (!hasReachedEndGracefully)
                                throw new MappingSerializationException(typeof(PropertyCollection), "missing a </properties> end tag");
                        }

                        else if (reader.CanReadElement("relations"))
                        {
                            bool hasReachedEndGracefully = false;
                            while (reader.Read())
                            {
                                if (reader.CanReadElement("property"))
                                {
                                    var prop = Read_PropertyElement(reader, config, "property", true) as Relation;
                                    if (prop != null)
                                        entMap.Relations.Add(prop);

                                }
                                else if (reader.HasReachedEndOfElement("relations"))
                                {
                                    hasReachedEndGracefully = true;
                                    break;
                                }
                            }

                            if (!hasReachedEndGracefully)
                                throw new MappingSerializationException(typeof(PropertyCollection), "missing a </relations> end tag");
                        }
                    }

                    throw new MappingSerializationException(typeof(EntityMap), "missing a </entity> end tag");
                }
                return null;
            }

            void Read_EntitiesElements(XmlReader reader, MapConfig config)
            {
                while (reader.Read())
                {
                    if (reader.CanReadElement("entity"))
                    {
                        var ent = Read_EntityElement(reader, config);
                        if (ent != null)
                        {
                            ent.Parent = config;
                            config.EntityConfigs.Add(ent);
                        }
                    }
                    else if (reader.HasReachedEndOfElement("entities"))
                        return;
                }
                throw new MappingSerializationException(typeof(EntityCollection), "missing a </entities> end tag");
            }

            void Read_ComplexTypesElement(XmlReader reader, MapConfig config)
            {
                while (reader.Read())
                {
                    if (reader.CanReadElement("type"))
                    {
                        var cp = this.Read_ComplexTypeElement(reader, config);
                        if (cp != null)
                            config.ComplexTypes.Add(cp);
                    }
                    else if (reader.HasReachedEndOfElement("complexTypes"))
                        return;
                }
                throw new MappingSerializationException(typeof(ComplexTypeCollection), "missing a </complexTypes> end tag");
            }

            void Read_GoDataElements(XmlReader reader, MapConfig config)
            {
                string previousElement = string.Empty;
                string currentElement = reader.Name;
                while (reader.Read())
                {
                    previousElement = currentElement;
                    currentElement = reader.Name;


                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (currentElement)
                        {
                            case "entities":
                                if (reader.CanReadElement("entities"))
                                    Read_EntitiesElements(reader, config);
                                break;
                            case "complexTypes":
                                if (reader.CanReadElement("complexTypes"))
                                    Read_ComplexTypesElement(reader, config);
                                break;
                            default:
                                break;
                        }
                    }

                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        switch (previousElement)
                        {
                            case "connectionString":
                                config.Settings.ConnectionString = reader.Value;
                                break;
                            case "tablePrefixes":
                                config.Settings.TablePrefixes = reader.Value;
                                break;
                            case "namespace":
                                config.Settings.Namespace = reader.Value;
                                break;
                            case "baseModel":
                                config.Settings.BaseModel = reader.Value;
                                break;
                            case "generatedBy":
                                config.GeneratedBy = reader.Value;
                                //reader.ReadEndElement();
                                break;
                            default:
                                break;
                        }
                    }

                    else if (reader.HasReachedEndOfElement("goliath.data"))
                        return;
                }

                throw new MappingSerializationException(typeof(MapConfig), "missing a </goliath.data> end tag");

            }

            void ProcessElement(XmlReader reader, MapConfig config)
            {
                if (reader.Name.Equals("goliath.data"))
                {
                    reader.MoveToFirstAttribute();
                    if (reader.Name.Equals("version"))
                        config.Settings.Version = reader.Value;

                    Read_GoDataElements(reader, config);
                }
            }

            #endregion
        }
    }
}

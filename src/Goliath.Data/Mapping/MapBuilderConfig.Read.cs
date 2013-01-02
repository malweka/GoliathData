using System;
using System.Collections.Generic;
using System.Xml;

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
                while (reader.ReadToFollowing("goliath.data", XmlNameSpace))
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

            #region <statement>

            StatementMap Read_StatementElement(XmlReader reader, MapConfig config, string elementName)
            {
                if (reader.CanReadElement(elementName))
                {
                    StatementMap statement = new StatementMap();

                    while (reader.MoveToNextAttribute())
                    {
                        string currentAttribute = reader.Name;
                        switch (currentAttribute)
                        {
                            case "name":
                                statement.Name = reader.Value;
                                break;
                            case "dbName":
                                statement.DbName = reader.Value;
                                break;
                            case "canRunOn":
                                statement.CanRunOn = reader.Value;
                                break;
                            case "resultMap":
                                statement.ResultMap = reader.Value;
                                break;
                            case "resultIsCollection":
                                statement.ResultIsCollection = ReadBool(reader.Value);
                                break;
                            case "inputParamType":
                                statement.InputParametersMap.Add("a", reader.Value);
                                break;
                            case "parse":
                                statement.IsParsingRequired = ReadBool(reader.Value);
                                break;
                            case "operationType":
                                statement.OperationType = ReadEnumType<MappedStatementType>(reader.Value);
                                break;
                            default:
                                break;
                        }
                    }

                    if ((statement.OperationType == MappedStatementType.Undefined) && !elementName.ToUpper().Equals("STATEMENT"))
                    {
                        statement.OperationType = ReadEnumType<MappedStatementType>(elementName);
                    }

                    return statement;
                }
                else
                {
                    return null;
                }
            }

            void Read_StatementsElement(XmlReader reader, MapConfig config, EntityMap entMap)
            {
                bool hasReachedEndGracefully = false;
                bool dependsOnEntity = false;
                if (entMap != null)
                {
                    dependsOnEntity = true;
                }
                while (reader.Read())
                {
                    StatementMap statement = null;
                    string elementName = reader.Name;

                    if (reader.CanReadElement("query") || reader.CanReadElement("insert") || reader.CanReadElement("statement") || reader.CanReadElement("update") || reader.CanReadElement("delete"))
                    {
                        statement = Read_StatementElement(reader, config, elementName);

                        if (string.IsNullOrEmpty(statement.Name))
                        {
                            if (dependsOnEntity)
                                statement.Name = StatementStore.BuildMappedStatementName(entMap, statement.OperationType);
                            else
                                throw new MappingSerializationException(typeof(StatementMap), "A statement in goliath.data/statements does not have name defined. A name cannot be infered.");

                        }

                        if (dependsOnEntity)
                        {
                            statement.DependsOnEntity = entMap.FullName;

                            if (string.IsNullOrEmpty(statement.ResultMap))
                            {
                                switch (statement.OperationType)
                                {
                                    case MappedStatementType.Update:
                                    case MappedStatementType.ExecuteNonQuery:
                                    case MappedStatementType.Insert:
                                        statement.ResultMap = typeof(Int32).ToString();
                                        break;
                                    case MappedStatementType.Query:
                                        statement.ResultMap = entMap.FullName;
                                        break;
                                    case MappedStatementType.Undefined:
                                        break;
                                    default:
                                        break;
                                }
                            }

                            if ((statement.InputParametersMap.Count == 0) && ((statement.OperationType == MappedStatementType.Insert) || (statement.OperationType == MappedStatementType.Update)))
                            {
                                statement.InputParametersMap.Add("a", entMap.FullName);
                            }
                        }

                        if (string.IsNullOrWhiteSpace(statement.CanRunOn))
                        {
                            //if can run on is empty we expect the platform to be on the main file.
                            if (string.IsNullOrWhiteSpace(config.Settings.Platform))
                                throw new MappingSerializationException(typeof(StatementMap), string.Format("Statement {0} missing canRunOn. Please specify which platform to run or specify one config rdbms.", statement.Name));

                            statement.CanRunOn = config.Settings.Platform;
                        }

                        reader.MoveToElement();

                        if (!reader.IsEmptyElement)
                        {
                            while (reader.Read())
                            {
                                if (reader.HasReachedEndOfElement(elementName))
                                    break;

                                if ((reader.NodeType == XmlNodeType.Text) || (reader.NodeType == XmlNodeType.CDATA))
                                {
                                    statement.Body = reader.Value;
                                }

                                else if (reader.CanReadElement("dbParameters"))
                                {
                                    while (reader.Read())
                                    {
                                        if (reader.HasReachedEndOfElement("dbParameters"))
                                            break;

                                        if (reader.CanReadElement("param"))
                                        {
                                            Read_StatementParams(reader, statement);
                                        }
                                    }
                                }
                                else if (reader.CanReadElement("inputParameters"))
                                {
                                    while (reader.Read())
                                    {
                                        if (reader.HasReachedEndOfElement("inputParameters"))
                                            break;

                                        if (reader.CanReadElement("input"))
                                        {
                                            Read_StatementInputParam(reader, statement);
                                        }
                                    }
                                }
                                else if (reader.CanReadElement("body"))
                                {
                                    while (reader.Read())
                                    {
                                        if (reader.HasReachedEndOfElement("body"))
                                            break;

                                        if ((reader.NodeType == XmlNodeType.Text) || (reader.NodeType == XmlNodeType.CDATA))
                                        {
                                            statement.Body = reader.Value;
                                        }
                                    }
                                }
                            }
                        }

                        if (statement.OperationType == MappedStatementType.Undefined)
                            throw new MappingSerializationException(typeof(StatementMap), string.Format("Statement {0} must have have an operationType", statement.Name));


                        config.UnprocessedStatements.Add(statement);

                    }

                    else if (reader.HasReachedEndOfElement("statements"))
                    {
                        hasReachedEndGracefully = true;
                        break;
                    }
                }

                if (!hasReachedEndGracefully)
                    throw new MappingSerializationException(typeof(StatementMap), "missing a </statements> end tag");
            }

            void Read_StatementInputParam(XmlReader reader, StatementMap statement)
            {
                string name = string.Empty;
                string type = string.Empty;

                while (reader.MoveToNextAttribute())
                {
                    string currentAttribute = reader.Name;
                    switch (currentAttribute)
                    {
                        case "name":
                            name = reader.Value;
                            break;
                        case "type":
                            type = reader.Value;
                            break;
                        default:
                            break;
                    }
                }

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type))
                {
                    throw new MappingSerializationException(typeof(StatementMap), string.Format("statement {0} - all parameters must have name and type attributes. {1} {2}", statement.Name, name, type));
                }

                statement.InputParametersMap.Add(name, type);
                reader.MoveToElement();
            }

            void Read_StatementParams(XmlReader reader, StatementMap statement)
            {
                string name = string.Empty;
                string property = string.Empty;

                while (reader.MoveToNextAttribute())
                {
                    string currentAttribute = reader.Name;
                    switch (currentAttribute)
                    {
                        case "name":
                            name = reader.Value;
                            break;
                        case "property":
                            property = reader.Value;
                            break;
                        default:
                            break;
                    }
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new MappingSerializationException(typeof(StatementMap), string.Format("statement {0} - all parameters must have name and value attributes. {1} {2}", statement.Name, name, property));
                }

                statement.DBParametersMap.Add(name, null);
                reader.MoveToElement();
            }

            #endregion

            #region <property>

            Property Read_PropertyElement(XmlReader reader, MapConfig config, string elementName, bool forceReturnRel = false)
            {
                return Read_PropertyElement(reader, config, elementName, null, forceReturnRel);
            }

            Property Read_PropertyElement(XmlReader reader, MapConfig config, string elementName, PrimaryKeyProperty pk, bool forceReturnRel = false)
            {
                if (reader.CanReadElement(elementName))
                {
                    Property property = new Property();
                    Relation rel = null;
                    string keyGen = null;
                    string unsavedValue = null;

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
                                //property.ClrType = ReadClrType(reader.Value);
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
                                property.IsComplexType = true;
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
                            case "referenceProperty":
                                InitializeRelObject(ref rel, property);
                                rel.ReferenceProperty = reader.Value;
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
                            case "inverse":
                                InitializeRelObject(ref rel, property);
                                rel.Inverse = ReadBool(reader.Value);
                                break;
                            case "mapColumn":
                                InitializeRelObject(ref rel, property);
                                rel.MapColumn = reader.Value;
                                break;
                            case "mapReferenceColumn":
                                InitializeRelObject(ref rel, property);
                                rel.MapReferenceColumn = reader.Value;
                                break;
                            case "propertyName":
                                InitializeRelObject(ref rel, property);
                                rel.MapPropertyName = reader.Value;
                                break;
                            case "mapTable":
                                InitializeRelObject(ref rel, property);
                                rel.MapTableName = reader.Value;
                                break;
                            //primary key attributes
                            case "unsaved_value":
                                unsavedValue = reader.Value;
                                break;
                            case "key_generator":
                                keyGen = reader.Value;
                                break;
                            default:
                                break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(elementName) && elementName.Equals("key", StringComparison.OrdinalIgnoreCase)
                        && pk != null)
                    {
                        pk.KeyGenerationStrategy = keyGen;
                        pk.UnsavedValueString = unsavedValue;
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

            #endregion

            #region <entity>

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
                            case "alias":
                                entMap.TableAlias = reader.Value;
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

                        //element primary key
                        else if (reader.CanReadElement("primaryKey"))
                        {
                            ElementEntityPrimaryKey(reader, config, entMap);
                        }

                        //element properties
                        else if (reader.CanReadElement("properties"))
                        {
                            ElementEntityProperties(reader, config, entMap);
                        }

                        //element relations
                        else if (reader.CanReadElement("relations"))
                        {
                            ElementEntityReadRelations(reader, config, entMap);
                        }

                        else if (reader.CanReadElement("statements"))
                        {
                            Read_StatementsElement(reader, config, entMap);
                        }
                    }

                    throw new MappingSerializationException(typeof(EntityMap), "missing a </entity> end tag");
                }
                return null;
            }

            void ElementEntityPrimaryKey(XmlReader reader, MapConfig config, EntityMap entMap)
            {
                bool hasReachedEndGracefully = false;
                List<PrimaryKeyProperty> keys = new List<PrimaryKeyProperty>();
                while (reader.Read())
                {
                    if (reader.CanReadElement("key"))
                    {
                        PrimaryKeyProperty pk = new PrimaryKeyProperty();
                        var prop = Read_PropertyElement(reader, config, "key", pk);
                        if (prop != null)
                        {
                            pk.Key = prop;
                            IKeyGenerator idGenerator;
                            if (!string.IsNullOrWhiteSpace(pk.KeyGenerationStrategy) && config.PrimaryKeyGeneratorStore.TryGetValue(pk.KeyGenerationStrategy, out idGenerator))
                            {
                                pk.KeyGenerator = idGenerator;
                            }
                            keys.Add(pk);
                        }

                    }
                    else if (reader.HasReachedEndOfElement("primaryKey"))
                    {
                        hasReachedEndGracefully = true;
                        break;
                    }
                }

                if (!hasReachedEndGracefully)
                    throw new MappingSerializationException(typeof(PrimaryKey), "missing a </primaryKey> end tag");

                entMap.AddKeyRange(keys);
            }

            void ElementEntityProperties(XmlReader reader, MapConfig config, EntityMap entMap)
            {
                bool hasReachedEndGracefully = false;
                while (reader.Read())
                {
                    if (reader.CanReadElement("property"))
                    {
                        var prop = Read_PropertyElement(reader, config, "property");
                        if (prop != null)
                            entMap.Add(prop);

                    }
                    else if (reader.CanReadElement("reference"))
                    {
                        var prop = Read_PropertyElement(reader, config, "reference", true) as Relation;
                        if (prop != null)
                            entMap.Add(prop);

                    }
                    else if (reader.CanReadElement("list"))
                    {
                        var prop = Read_List(reader, config, CollectionType.List);
                        if (prop != null)
                        {
                            entMap.Add(prop);
                        }

                    }
                    else if (reader.CanReadElement("map"))
                    {
                        var prop = Read_List(reader, config, CollectionType.Map);
                        if (prop != null)
                        {
                            entMap.Add(prop);
                        }

                    }
                    else if (reader.CanReadElement("set"))
                    {
                        var prop = Read_List(reader, config, CollectionType.Set);
                        if (prop != null)
                        {
                            entMap.Add(prop);
                        }

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

            void ElementEntityReadRelations(XmlReader reader, MapConfig config, EntityMap entMap)
            {
                bool hasReachedEndGracefully = false;
                while (reader.Read())
                {
                    if (reader.CanReadElement("property"))
                    {
                        var prop = Read_PropertyElement(reader, config, "property", true) as Relation;
                        if (prop != null)
                            entMap.Add(prop);

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


            Relation Read_List(XmlReader reader, MapConfig config, CollectionType listType)
            {
                Relation rel = null;
                string elementName = null;
                switch (listType)
                {
                    case CollectionType.List:
                        elementName = "list";
                        break;
                    case CollectionType.Map:
                        elementName = "map";
                        break;
                    case CollectionType.Set:
                        elementName = "set";
                        break;
                    default:
                        return null;
                }

                rel = Read_PropertyElement(reader, config, elementName, true) as Relation;
                rel.CollectionType = listType;
                return rel;
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

            #endregion

            #region <complexType>

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

            #endregion

            void Read_Project_PropertiesElement(XmlReader reader, MapConfig config)
            {
                //bool hasReachedEndGracefully = false;
                while (reader.Read())
                {
                    if (reader.CanReadElement("property"))
                    {
                        string propName = null;
                        string propValue = null;
                        while (reader.MoveToNextAttribute())
                        {
                            var currAttribute = reader.Name;

                            switch (currAttribute)
                            {
                                case "name":
                                    propName = reader.Value;
                                    break;
                                case "value":
                                    propValue = reader.Value;
                                    break;
                                default:
                                    break;
                            }

                        }

                        if (!string.IsNullOrWhiteSpace(propName))
                            config.Settings.SetPropety(propName, propValue);
                    }
                    else if (reader.HasReachedEndOfElement("project.properties"))
                        return;
                }
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
                            case "project.properties":
                                if (reader.CanReadElement("project.properties"))
                                    Read_Project_PropertiesElement(reader, config);
                                break;
                            case "statements":
                                if (reader.CanReadElement("statements"))
                                    Read_StatementsElement(reader, config, null);
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
                                config.Settings.GeneratedBy = reader.Value;
                                break;
                            //case "rdbms":
                            //    ReadRdbms(reader, config);
                            //    break;
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
                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.Name)
                        {
                            case "version":
                                config.Settings.Version = reader.Value;
                                break;
                            case "rdbms":
                                config.Settings.Platform = reader.Value;
                                break;
                            default:
                                break;
                        }
                    }


                    Read_GoDataElements(reader, config);
                }
            }

            #endregion
        }
    }
}

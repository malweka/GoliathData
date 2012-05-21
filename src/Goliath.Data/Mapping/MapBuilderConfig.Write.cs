using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;
namespace Goliath.Data.Mapping
{
    public partial class MapConfig
    {

        /// <summary>
        /// Saves the model into the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="readable">if set to <c>true</c> the file will be formated to be readable by humans.</param>
        public void Save(Stream stream, bool readable)
        {
            using (XmlTextWriter xmlWriter = new XmlTextWriter(stream, Encoding.UTF8))
            {
                if (readable)
                    xmlWriter.Formatting = Formatting.Indented;
                else
                    xmlWriter.Formatting = Formatting.None;

                xmlWriter.WriteStartElement("goliath.data", XmlNameSpace);

                if ((Settings != null) && !string.IsNullOrEmpty(Settings.Platform))
                {
                    xmlWriter.WriteStartAttribute("rdbms");
                    xmlWriter.WriteString(Settings.Platform);
                    xmlWriter.WriteEndAttribute();
                }

                xmlWriter.WriteStartAttribute("version");
                xmlWriter.WriteString(this.GetType().Assembly.GetName().Version.ToString());
                xmlWriter.WriteEndAttribute();

                xmlWriter.WriteStartElement("connectionString");
                xmlWriter.WriteString(Settings.ConnectionString);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("tablePrefixes");
                xmlWriter.WriteString(Settings.TablePrefixes);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("namespace");
                xmlWriter.WriteString(Settings.Namespace);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("baseModel");
                xmlWriter.WriteString(Settings.BaseModel);
                xmlWriter.WriteEndElement();

                if ((Settings != null) && !string.IsNullOrWhiteSpace(Settings.GeneratedBy))
                {
                    xmlWriter.WriteStartElement("generatedBy");
                    xmlWriter.WriteString(Settings.GeneratedBy);
                    xmlWriter.WriteEndElement();
                }

                if (Settings.Properties.Count > 0)
                {
                    xmlWriter.WriteStartElement("project.properties");
                    foreach (var prop in Settings.Properties)
                    {
                        xmlWriter.WriteStartElement("property");
                        xmlWriter.WriteStartAttribute("name");
                        xmlWriter.WriteString(prop.Key);
                        xmlWriter.WriteEndAttribute();
                        xmlWriter.WriteStartAttribute("value");
                        xmlWriter.WriteString(prop.Value.ToString());
                        xmlWriter.WriteEndAttribute();
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteStartElement("entities");
                foreach (var entity in EntityConfigs)
                {
                    xmlWriter.WriteStartElement("entity");

                    xmlWriter.WriteStartAttribute("name");
                    xmlWriter.WriteString(entity.Name);
                    xmlWriter.WriteEndAttribute();

                    if (!string.IsNullOrWhiteSpace(entity.Extends))
                    {
                        xmlWriter.WriteStartAttribute("extends");
                        xmlWriter.WriteString(entity.Extends);
                        xmlWriter.WriteEndAttribute();
                    }

                    if (entity.IsLinkTable)
                    {
                        xmlWriter.WriteStartAttribute("linkTable");
                        xmlWriter.WriteString(entity.IsLinkTable.ToString());
                        xmlWriter.WriteEndAttribute();
                    }

                    xmlWriter.WriteStartAttribute("assembly");
                    xmlWriter.WriteString(entity.AssemblyName);
                    xmlWriter.WriteEndAttribute();

                    xmlWriter.WriteStartAttribute("entityNamespace");
                    xmlWriter.WriteString(entity.Namespace);
                    xmlWriter.WriteEndAttribute();

                    xmlWriter.WriteStartAttribute("table");
                    xmlWriter.WriteString(entity.TableName);
                    xmlWriter.WriteEndAttribute();

                    xmlWriter.WriteStartAttribute("schema");
                    xmlWriter.WriteString(entity.SchemaName);
                    xmlWriter.WriteEndAttribute();

                    xmlWriter.WriteStartAttribute("alias");
                    xmlWriter.WriteString(entity.TableAlias);
                    xmlWriter.WriteEndAttribute();

                    //primary key
                    if (entity.PrimaryKey != null)
                    {
                        xmlWriter.WriteStartElement("primaryKey");
                        WritePrimaryKey(xmlWriter, entity.PrimaryKey);
                        xmlWriter.WriteEndElement();
                    }

                    WriteTransformations(xmlWriter, entity);

                    xmlWriter.WriteEndElement();//entity
                }
                xmlWriter.WriteEndElement();//end entities

                xmlWriter.WriteStartElement("complexTypes");
                foreach (var complex in ComplexTypes)
                {
                    xmlWriter.WriteStartElement("type");

                    xmlWriter.WriteStartAttribute("fullname");
                    xmlWriter.WriteString(complex.FullName);
                    xmlWriter.WriteEndAttribute();

                    xmlWriter.WriteStartAttribute("enum");
                    xmlWriter.WriteString(complex.IsEnum.ToString());
                    xmlWriter.WriteEndAttribute();

                    WriteTransformations(xmlWriter, complex);

                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();//end complexTypes

                xmlWriter.WriteEndElement();//end Goliath.Data
            }
        }

        void WritePrimaryKey(XmlTextWriter xmlWriter, PrimaryKey pk)
        {
            foreach (var key in pk.Keys)
            {
                xmlWriter.WriteStartElement("key");
                WriteTransformations(xmlWriter, key.Key, "key", false);
                if (!string.IsNullOrWhiteSpace(key.UnsavedValue))
                {
                    xmlWriter.WriteStartAttribute("unsaved_value");
                    xmlWriter.WriteString(key.UnsavedValue);
                    xmlWriter.WriteEndAttribute();
                }
                if (!string.IsNullOrWhiteSpace(key.KeyGenerationStrategy))
                {
                    xmlWriter.WriteStartAttribute("key_generator");
                    xmlWriter.WriteString(key.KeyGenerationStrategy);
                    xmlWriter.WriteEndAttribute();
                }
                xmlWriter.WriteEndElement();
            }
        }

        void WriteTransformations(XmlTextWriter xmlWriter, Property transformation, string elementName = "property", bool closeElement = true)
        {
            if (closeElement)
                xmlWriter.WriteStartElement(elementName);

            xmlWriter.WriteStartAttribute("name");
            xmlWriter.WriteString(transformation.PropertyName);
            xmlWriter.WriteEndAttribute();

            if (!string.IsNullOrWhiteSpace(transformation.ColumnName))
            {
                xmlWriter.WriteStartAttribute("column");
                xmlWriter.WriteString(transformation.ColumnName);
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.ConstraintType > 0)
            {
                xmlWriter.WriteStartAttribute("constraintType");
                xmlWriter.WriteString(transformation.ConstraintType.ToString());
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.ClrType != null)
            {
                xmlWriter.WriteStartAttribute("clrType");
                xmlWriter.WriteString(transformation.ClrType.ToString());
                xmlWriter.WriteEndAttribute();
            }           


            if (!string.IsNullOrWhiteSpace(transformation.SqlType))
            {
                xmlWriter.WriteStartAttribute("sqlType");
                xmlWriter.WriteString(transformation.SqlType);
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.Length > 0)
            {
                xmlWriter.WriteStartAttribute("length");
                xmlWriter.WriteString(transformation.Length.ToString());
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.Precision > 0)
            {
                xmlWriter.WriteStartAttribute("precision");
                xmlWriter.WriteString(transformation.Precision.ToString());
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.Scale >= 0)
            {
                xmlWriter.WriteStartAttribute("scale");
                xmlWriter.WriteString(transformation.Scale.ToString());
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.IsComplexType)
            {
                xmlWriter.WriteStartAttribute("type");
                xmlWriter.WriteString(transformation.ComplexTypeName);
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.LazyLoad)
            {
                xmlWriter.WriteStartAttribute("lazy");
                xmlWriter.WriteString(transformation.LazyLoad.ToString());
                xmlWriter.WriteEndAttribute();
            }

            //xmlWriter.WriteStartAttribute("abbr");
            //xmlWriter.WriteString(transformation.Abbreviation);
            //xmlWriter.WriteEndAttribute();

            //if (transformation.IsNullable)
            //{
            //   xmlWriter.WriteStartAttribute("nullable");
            //   xmlWriter.WriteString(transformation.IsNullable.ToString());
            //   xmlWriter.WriteEndAttribute();
            //}

            //if (transformation.IsIdentity)
            //{
            //   xmlWriter.WriteStartAttribute("identity");
            //   xmlWriter.WriteString(transformation.IsIdentity.ToString());
            //   xmlWriter.WriteEndAttribute();
            //}

            //if (transformation.IsAutoGenerated)
            //{
            //   xmlWriter.WriteStartAttribute("autoGenerated");
            //   xmlWriter.WriteString(transformation.IsAutoGenerated.ToString());
            //   xmlWriter.WriteEndAttribute();
            //}

            if (transformation.IsUnique)
            {
                xmlWriter.WriteStartAttribute("unique");
                xmlWriter.WriteString(transformation.IsUnique.ToString());
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.IsPrimaryKey)
            {
                xmlWriter.WriteStartAttribute("primaryKey");
                xmlWriter.WriteString(transformation.IsPrimaryKey.ToString());
                xmlWriter.WriteEndAttribute();
            }

            if (!string.IsNullOrWhiteSpace(transformation.DefaultValue))
            {
                xmlWriter.WriteStartAttribute("default");
                xmlWriter.WriteString(transformation.DefaultValue);
                xmlWriter.WriteEndAttribute();
            }

            if (!string.IsNullOrWhiteSpace(transformation.ConstraintName))
            {
                xmlWriter.WriteStartAttribute("constraint");
                xmlWriter.WriteString(transformation.ConstraintName);
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.IgnoreOnUpdate)
            {
                xmlWriter.WriteStartAttribute("ignoreOnUpdate");
                xmlWriter.WriteString(transformation.IgnoreOnUpdate.ToString());
                xmlWriter.WriteEndAttribute();
            }

            if (!transformation.IsNullable)
            {
                xmlWriter.WriteStartAttribute("nullable");
                xmlWriter.WriteString(transformation.IsNullable.ToString());
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.IsIdentity)
            {
                xmlWriter.WriteStartAttribute("identity");
                xmlWriter.WriteString(transformation.IsIdentity.ToString());
                xmlWriter.WriteEndAttribute();
            }

            if (transformation.IsAutoGenerated)
            {
                xmlWriter.WriteStartAttribute("autoGenerated");
                xmlWriter.WriteString(transformation.IsAutoGenerated.ToString());
                xmlWriter.WriteEndAttribute();
            }

            if (transformation is Relation)
            {
                Relation rel = (Relation)transformation;
                xmlWriter.WriteStartAttribute("relation");
                xmlWriter.WriteString(rel.RelationType.ToString());
                xmlWriter.WriteEndAttribute();

                //xmlWriter.WriteStartAttribute("referenceTable");
                //xmlWriter.WriteString(rel.ReferenceTable);
                //xmlWriter.WriteEndAttribute();

                if (!string.IsNullOrWhiteSpace(rel.ReferenceColumn))
                {
                    //xmlWriter.WriteStartAttribute("referenceColumn");
                    //xmlWriter.WriteString(rel.ReferenceColumn);
                    //xmlWriter.WriteEndAttribute();
                }

                if (!string.IsNullOrWhiteSpace(rel.ReferenceProperty))
                {
                    xmlWriter.WriteStartAttribute("referenceProperty");
                    xmlWriter.WriteString(rel.ReferenceProperty);
                    xmlWriter.WriteEndAttribute();
                }

                if (!string.IsNullOrWhiteSpace(rel.ReferenceConstraintName))
                {
                    xmlWriter.WriteStartAttribute("refConstraint");
                    xmlWriter.WriteString(rel.ReferenceConstraintName);
                    xmlWriter.WriteEndAttribute();
                }

                if (!string.IsNullOrWhiteSpace(rel.ReferenceEntityName))
                {
                    xmlWriter.WriteStartAttribute("refEntity");
                    xmlWriter.WriteString(rel.ReferenceEntityName);
                    xmlWriter.WriteEndAttribute();
                }

                if (rel.Exclude)
                {
                    xmlWriter.WriteStartAttribute("exclude");
                    xmlWriter.WriteString(rel.Exclude.ToString());
                    xmlWriter.WriteEndAttribute();
                }

                if ((rel.RelationType == RelationshipType.ManyToMany) && (rel.Inverse))
                {
                    xmlWriter.WriteStartAttribute("inverse");
                    xmlWriter.WriteString(rel.Inverse.ToString());
                    xmlWriter.WriteEndAttribute();
                }

                if (rel.RelationType == RelationshipType.ManyToOne)
                {
                    xmlWriter.WriteStartAttribute("dbType");
                    xmlWriter.WriteString(transformation.DbType.ToString());
                    xmlWriter.WriteEndAttribute();
                }

            }
            else
            {
                xmlWriter.WriteStartAttribute("dbType");
                xmlWriter.WriteString(transformation.DbType.ToString());
                xmlWriter.WriteEndAttribute();
            }

            //if (!string.IsNullOrWhiteSpace(transformation.Errors))
            //{
            //    xmlWriter.WriteStartElement("errors");
            //    xmlWriter.WriteCData(transformation.Errors);
            //    xmlWriter.WriteEndElement();//end errors
            //}

            if (closeElement)
                xmlWriter.WriteEndElement();//end property
        }

        void WriteTransformations(XmlTextWriter xmlWriter, EntityMap entity)
        {
            xmlWriter.WriteStartElement("properties");
            foreach (var trans in entity.Properties)
            {
                WriteTransformations(xmlWriter, trans);
            }

            foreach (var rel in entity.Relations)
            {
                if (rel.CollectionType == CollectionType.None)
                    WriteTransformations(xmlWriter, rel, "reference");
                else
                    WriteList(xmlWriter, rel);
            }
            xmlWriter.WriteEndElement();//relations
        }

        void WriteList(XmlTextWriter xmlWriter, Relation relation)
        {
            switch (relation.CollectionType)
            {
                case CollectionType.List:
                    xmlWriter.WriteStartElement("list");
                    break;
                case CollectionType.Map:
                    xmlWriter.WriteStartElement("map");
                    break;
                case CollectionType.Set:
                    xmlWriter.WriteStartElement("set");
                    break;
                default:
                    return;
            }

            WriteTransformations(xmlWriter, relation, "list", false);

            if (relation.RelationType == RelationshipType.ManyToMany)
            {                

                if (!string.IsNullOrWhiteSpace(relation.MapTableName))
                {
                    xmlWriter.WriteStartAttribute("mapTable");
                    xmlWriter.WriteString(relation.MapTableName);
                    xmlWriter.WriteEndAttribute();
                }

                if (!string.IsNullOrWhiteSpace(relation.MapColumn))
                {
                    xmlWriter.WriteStartAttribute("mapColumn");
                    xmlWriter.WriteString(relation.MapColumn);
                    xmlWriter.WriteEndAttribute();
                }

                if (!string.IsNullOrWhiteSpace(relation.MapReferenceColumn))
                {
                    xmlWriter.WriteStartAttribute("mapReferenceColumn");
                    xmlWriter.WriteString(relation.MapReferenceColumn);
                    xmlWriter.WriteEndAttribute();
                }
            }            
            xmlWriter.WriteEndElement();
        }

        void WriteTransformations(XmlTextWriter xmlWriter, ComplexType cType)
        {
            xmlWriter.WriteStartElement("properties");
            foreach (var trans in cType.Properties)
            {
                WriteTransformations(xmlWriter, trans);
            }

            
            xmlWriter.WriteEndElement();//relations
        }

        //string WritePlatformText(string platform)
        //{
        //    List<string> list = new List<string>();

        //    if ((platform & SupportedRdbms.Mssql2005) == SupportedRdbms.Mssql2005)
        //    {
        //        list.Add(SupportedRdbms.Mssql2005.ToString());
        //    }

        //    if ((platform & SupportedRdbms.Mssql2008) == SupportedRdbms.Mssql2008)
        //    {
        //        list.Add(SupportedRdbms.Mssql2008.ToString());
        //    }

        //    if ((platform & SupportedRdbms.Mssql2008R2) == SupportedRdbms.Mssql2008R2)
        //    {
        //        list.Add(SupportedRdbms.Mssql2008R2.ToString());
        //    }

        //    if ((platform & SupportedRdbms.Sqlite3) == SupportedRdbms.Sqlite3)
        //    {
        //        list.Add(SupportedRdbms.Sqlite3.ToString());
        //    }

        //    if ((platform & SupportedRdbms.Postgresql9) == SupportedRdbms.Postgresql9)
        //    {
        //        list.Add(SupportedRdbms.Postgresql9.ToString());
        //    }

        //    if ((platform & SupportedRdbms.MySql5) == SupportedRdbms.MySql5)
        //    {
        //        list.Add(SupportedRdbms.MySql5.ToString());
        //    }

        //    return string.Join(",", list);
        //}
    }
}

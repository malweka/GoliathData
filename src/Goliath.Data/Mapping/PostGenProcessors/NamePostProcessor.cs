using System;
using System.Collections.Generic;
using System.Linq;
using Goliath.Data.Transformers;
using Goliath.Data.Utils;
using Microsoft.SqlServer.Server;

namespace Goliath.Data.Mapping
{
    class NamePostProcessor : IPostGenerationProcessor
    {
        NameTransformerFactory transfactory;
        ITableNameAbbreviator tableAbbreviator;

        public NamePostProcessor(NameTransformerFactory transformerFactory, ITableNameAbbreviator tableAbbreviator)
        {
            if (transformerFactory == null)
                throw new ArgumentNullException("transformerFactory");
            if (tableAbbreviator == null)
                throw new ArgumentNullException("tableAbbreviator");

            transfactory = transformerFactory;
            this.tableAbbreviator = tableAbbreviator;
        }

        #region IPostGenerationProcessor Members

        public void Process(IDictionary<string, EntityMap> entities, StatementStore mappedStatementStore, IDictionary<string, string> entityRenames)
        {
            ProcessTableNames(entities, entityRenames);
        }

        #endregion

        void ProcessTableNames(IDictionary<string, EntityMap> entities, IDictionary<string, string> entityRenames)
        {
            var tableNamer = transfactory.GetTransformer<EntityMap>();
            var relNamer = transfactory.GetTransformer<Relation>();
            var propNamer = transfactory.GetTransformer<Property>();

            foreach (var table in entities.Values)
            {
                table.Name = GetRename(tableNamer.Transform(table, table.Name), entityRenames);
                table.TableAlias = tableAbbreviator.Abbreviate(table.Name);
                var propertyListClone = table.ToArray();

                foreach (var prop in propertyListClone)
                {

                    if (prop is Relation)
                    {
                        var rel = (Relation)prop;

                        EntityMap refEnt;
                        if (entities.TryGetValue(rel.ReferenceTable, out refEnt))
                        {
                            rel.ReferenceEntityName = string.Format("{0}.{1}", refEnt.Namespace, GetRename(tableNamer.Transform(null, rel.ReferenceTable), entityRenames));
                        }

                        string name = relNamer.Transform(rel, rel.ColumnName);
                        if (!string.IsNullOrWhiteSpace(rel.MapPropertyName))
                        {
                            string mapPropName = propNamer.Transform(rel, rel.MapPropertyName);
                            rel.MapPropertyName = mapPropName;
                        }

                        if (!string.Equals(rel.ColumnName, name) && !rel.IsPrimaryKey)
                        {
                            if (rel.RelationType == RelationshipType.ManyToOne)
                            {
                                Property newProperty = rel.Clone();
                                newProperty.PropertyName = rel.ColumnName.Pascalize();
                                table.Remove(rel);
                                rel.PropertyName = name;

                                if (rel.PropertyName.Equals(newProperty.PropertyName))
                                {
                                    rel.PropertyName = string.Concat(name, "Entity");
                                }

                                table.Add(rel);
                                table.Add(newProperty);
                            }
                        }
                        else
                            rel.PropertyName = name;

                        if (!string.IsNullOrWhiteSpace(rel.ReferenceProperty))
                        {
                            rel.ReferenceProperty = propNamer.Transform(rel, rel.ReferenceProperty);
                        }
                    }
                    else
                    {
                        prop.PropertyName = propNamer.Transform(prop, prop.ColumnName);
                    }

                    if (prop.PropertyName.Equals(table.Name))
                    {
                        //member name cannot be the same as enclosing type. We rename
                        prop.PropertyName = prop.PropertyName + "Property";
                    }
                }

                if (!table.IsLinkTable && table.PrimaryKey != null)
                {
                    foreach (var pk in table.PrimaryKey.Keys)
                    {
                        if (pk.Key.DbType == System.Data.DbType.Guid)
                        {
                            pk.KeyGenerationStrategy = Generators.GuidCombGenerator.GeneratorName;
                            pk.UnsavedValueString = Guid.Empty.ToString();
                        }
                        else if (pk.Key.IsIdentity)
                        {
                            pk.KeyGenerationStrategy = Generators.AutoIncrementGenerator.GeneratorName;
                            pk.UnsavedValueString = "0";
                        }
                    }
                }

            }
        }


        string GetRename(string tableName, IDictionary<string, string> entityRenames)
        {
            if (entityRenames == null) return tableName;

            string newName;
            if (entityRenames.TryGetValue(tableName, out newName))
                return newName;

            return tableName;
        }

    }
}

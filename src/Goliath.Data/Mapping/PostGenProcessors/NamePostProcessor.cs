using System;
using System.Collections.Generic;
using System.Linq;
using Goliath.Data.Transformers;
using Goliath.Data.Utils;

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

        public void Process(IDictionary<string, EntityMap> entities)
        {
            ProcessTableNames(entities);
        }

        #endregion

        void ProcessTableNames(IDictionary<string, EntityMap> entities)
        {
            var tableNamer = transfactory.GetTransformer<EntityMap>();
            var relNamer = transfactory.GetTransformer<Relation>();
            var propNamer = transfactory.GetTransformer<Property>();

            foreach (var table in entities.Values)
            {
                table.Name = tableNamer.Transform(table, table.Name);
                table.TableAlias = tableAbbreviator.Abbreviate(table.Name);
                var propertyListClone = table.ToArray();

                foreach (var prop in propertyListClone)
                {
                    //var prop = table[i];

                    if (prop is Relation)
                    {
                        var rel = (Relation)prop;

                        EntityMap refEnt;
                        if (entities.TryGetValue(rel.ReferenceTable, out refEnt))
                        {
                            rel.ReferenceEntityName = string.Format("{0}.{1}", refEnt.Namespace, tableNamer.Transform(null, rel.ReferenceTable));
                        }

                        string name = relNamer.Transform(rel, rel.ColumnName);
                        if(!string.IsNullOrWhiteSpace(rel.MapPropertyName))
                        {
                            string mapPropName = propNamer.Transform(rel, rel.MapPropertyName);
                            rel.MapPropertyName = mapPropName;
                        }

                        if (!string.Equals(rel.ColumnName, name) && !rel.IsPrimaryKey)
                        {
                            //rel.PropertyName = name;
                            if (rel.RelationType == RelationshipType.ManyToOne)
                            {
                                Property newProperty = rel.Clone();
                                newProperty.PropertyName = rel.ColumnName.Pascalize();
                                table.Remove(rel);
                                rel.PropertyName = name;
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
                }

                if (!table.IsLinkTable && table.PrimaryKey != null)
                {
                    foreach (var pk in table.PrimaryKey.Keys)
                    {
                        if(pk.Key.DbType == System.Data.DbType.Guid)
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


    }
}

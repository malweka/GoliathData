using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Utils;

namespace Goliath.Data.Mapping
{
    class RelationshipProcessor : IPostGenerationProcessor
    {
        public virtual void Process(IDictionary<string, EntityMap> entityList)
        {
            foreach (var ent in entityList.Values)
            {
                try
                {
                    //find link tables
                    if ((ent.Properties.Count == 0) && (ent.Relations.Count == 0)
                       && (ent.PrimaryKey != null) && (ent.PrimaryKey.Keys.Count >= 2) && ent.PrimaryKey.Keys[0].Key.IsPrimaryKey && ent.PrimaryKey.Keys[1].Key.IsPrimaryKey)
                    {

                        var aRel = ent.PrimaryKey.Keys[0].Key as Relation;
                        var bRel = ent.PrimaryKey.Keys[1].Key as Relation;

                        if ((aRel == null) || (bRel == null))
                            continue;

                        ent.IsLinkTable = true;

                        EntityMap aEnt;
                        EntityMap bEnt;
                        if (entityList.TryGetValue(aRel.ReferenceTable, out aEnt))
                        {
                            if (entityList.TryGetValue(bRel.ReferenceTable, out bEnt))
                            {
                                aEnt.Relations.Add(new Relation()
                                {
                                    IsComplexType = true,
                                    LazyLoad = true,
                                    ColumnName = aRel.ReferenceColumn ?? string.Empty,
                                    ReferenceColumn = bRel.ReferenceColumn ?? string.Empty,
                                    CollectionType = Mapping.CollectionType.List,
                                    MapTableName = ent.TableName,
                                    MapColumn = aRel.ColumnName ?? string.Empty,
                                    PropertyName = string.Format("{0}On{1}_{2}", bEnt.Name.Pluralize(), ent.Name, aRel.ColumnName),
                                    ReferenceEntityName = bEnt.FullName,
                                    ReferenceTable = bEnt.TableName,
                                    RelationType = RelationshipType.ManyToMany,
                                });

                                bEnt.Relations.Add(new Relation()
                                {
                                    IsComplexType = true,
                                    ColumnName = bRel.ReferenceColumn ?? string.Empty,
                                    ReferenceColumn = aRel.ReferenceColumn ?? string.Empty,
                                    MapTableName = ent.TableName,
                                    MapColumn = bRel.ColumnName ?? string.Empty,
                                    CollectionType = Mapping.CollectionType.List,
                                    LazyLoad = true,
                                    PropertyName = string.Format("{0}On{1}_{2}", aEnt.Name.Pluralize(), ent.Name, bRel.ColumnName),
                                    ReferenceEntityName = aEnt.FullName,
                                    ReferenceTable = aEnt.TableName,
                                    RelationType = RelationshipType.ManyToMany,
                                });
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < ent.Relations.Count; i++)
                        {
                            var reference = ent.Relations[i];
                            if (reference.RelationType != RelationshipType.ManyToOne)
                            {
                                continue;
                            }

                            EntityMap other;
                            if (entityList.TryGetValue(reference.ReferenceTable, out other))
                            {
                                if (reference.IsPrimaryKey)
                                {
                                    //we have a one to one.
                                    ent.Extends = other.FullName;
                                }
                                else
                                {
                                    other.Relations.Add(new Relation()
                                    {
                                        IsComplexType = true,
                                        LazyLoad = true,
                                        ColumnName = reference.ReferenceColumn,
                                        ReferenceColumn = reference.ColumnName,
                                        PropertyName = string.Format("{0}On{1}", ent.Name.Pluralize(), reference.ColumnName),
                                        ReferenceTable = ent.TableName,
                                        RelationType = RelationshipType.OneToMany,
                                        ReferenceEntityName = ent.FullName,
                                        CollectionType = CollectionType.List, 
                                    });
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}

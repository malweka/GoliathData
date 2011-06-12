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
                    if ((ent.Properties.Count == 0) && (ent.Relations.Count >= 2)
                       && ent.Relations[0].IsPrimaryKey && ent.Relations[1].IsPrimaryKey)
                    {
                        ent.IsLinkTable = true;
                        var aRel = ent.Relations[0];
                        var bRel = ent.Relations[1];

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
                                    ColumnName = aRel.ColumnName ?? string.Empty,
                                    PropertyName = string.Format("{0}On{1}_{2}", bEnt.Name.Pluralize(), ent.Name, aRel.ColumnName),
                                    ReferenceEntityName = bEnt.Name,
                                    ComplexTypeName = "IList",
                                    ReferenceTable = bEnt.TableName,
                                    RelationType = RelationshipType.ManyToMany,
                                });

                                bEnt.Relations.Add(new Relation()
                                {
                                    IsComplexType = true,
                                    ColumnName = bRel.ColumnName ?? string.Empty,
                                    LazyLoad = true,
                                    PropertyName = string.Format("{0}On{1}_{2}", aEnt.Name.Pluralize(), ent.Name, bRel.ColumnName),
                                    ComplexTypeName = "IList",
                                    ReferenceEntityName = aEnt.Name,
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
                            if (reference.RelationType != RelationshipType.OneToMany)
                                continue;

                            EntityMap other;
                            if (entityList.TryGetValue(reference.ReferenceTable, out other))
                            {
                                if (reference.IsPrimaryKey)
                                {
                                    //we have a one to one.
                                    ent.Extends = other.Name;
                                }
                                else
                                {
                                    other.Relations.Add(new Relation()
                                    {
                                        IsComplexType = true,
                                        LazyLoad = true,
                                        ColumnName = reference.ColumnName,
                                        PropertyName = string.Format("{0}On{1}", ent.Name.Pluralize(), reference.ColumnName),
                                        ComplexTypeName = "IList",
                                        ReferenceTable = ent.TableName,
                                        RelationType = RelationshipType.ManyToOne,
                                        ReferenceEntityName = ent.Name,
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

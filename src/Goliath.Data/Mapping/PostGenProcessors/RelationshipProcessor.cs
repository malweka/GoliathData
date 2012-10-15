using System.Collections.Generic;

namespace Goliath.Data.Mapping
{
    using Utils;

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
                                    ReferenceProperty = bRel.ReferenceProperty ?? string.Empty,
                                    CollectionType = Mapping.CollectionType.List,
                                    MapTableName = ent.TableName,
                                    MapColumn = aRel.ColumnName ?? string.Empty,
                                    MapReferenceColumn = bRel.ColumnName ?? string.Empty,
                                    PropertyName = string.Format("{0}On{1}_{2}", bEnt.Name.Pluralize(), ent.Name, aRel.ColumnName.Pascalize()),
                                    ReferenceEntityName = bEnt.FullName,
                                    ReferenceTable = bEnt.TableName,
                                    RelationType = RelationshipType.ManyToMany,
                                    Inverse = true,
                                });

                                bEnt.Relations.Add(new Relation()
                                {
                                    IsComplexType = true,
                                    ColumnName = bRel.ReferenceColumn ?? string.Empty,
                                    ReferenceColumn = aRel.ReferenceColumn ?? string.Empty,
                                    ReferenceProperty = aRel.ReferenceProperty ?? string.Empty,
                                    MapTableName = ent.TableName,
                                    MapColumn = bRel.ColumnName ?? string.Empty,
                                    MapReferenceColumn = aRel.ColumnName ?? string.Empty,
                                    CollectionType = Mapping.CollectionType.List,
                                    LazyLoad = true,
                                    PropertyName = string.Format("{0}On{1}_{2}", aEnt.Name.Pluralize(), ent.Name, bRel.ColumnName.Pascalize()),
                                    ReferenceEntityName = aEnt.FullName,
                                    ReferenceTable = aEnt.TableName,
                                    RelationType = RelationshipType.ManyToMany,
                                });
                            }
                        }
                    }
                    else
                    {
                        if ((ent.PrimaryKey != null) && (ent.PrimaryKey.Keys.Count == 1))
                        {
                            var key = ent.PrimaryKey.Keys[0];
                            if ((key != null) && (key.Key is Relation))
                            {
                                var pk = (Relation)key.Key;
                                ent.Extends = pk.ReferenceEntityName;
                                //ent.IsSubClass = true;
                            }
                        }

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
                                other.Relations.Add(new Relation()
                                {
                                    IsComplexType = true,
                                    LazyLoad = true,
                                    ColumnName = reference.ReferenceColumn,
                                    ReferenceColumn = reference.ColumnName,
                                    ReferenceProperty = reference.PropertyName,
                                    PropertyName = string.Format("{0}On{1}", ent.Name.Pluralize(), reference.ColumnName.Pascalize()),
                                    ReferenceTable = ent.TableName,
                                    RelationType = RelationshipType.OneToMany,
                                    ReferenceEntityName = ent.FullName,
                                    CollectionType = CollectionType.List,
                                });
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Goliath.Data.Providers;

namespace Goliath.Data.Mapping
{
    using Utils;

    class RelationshipProcessor : IPostGenerationProcessor
    {
        static readonly ILogger logger;
        private SqlDialect dialect;
        private readonly ProjectSettings settings;

        static RelationshipProcessor()
        {
            logger = Logger.GetLogger(typeof(RelationshipProcessor));
        }

        public RelationshipProcessor(SqlDialect dialect, ProjectSettings settings)
        {
            this.dialect = dialect;
            this.settings = settings;
        }

        public virtual void Process(IDictionary<string, EntityMap> entityList, StatementStore mappedStatementStore, IDictionary<string, string> entityRenames)
        {
            foreach (var ent in entityList.Values)
            {
                try
                {
                    //find link tables
                    if (settings.SupportManyToMany && ent.IsLinkTable)
                    {
                        var aRel = ent.PrimaryKey.Keys[0].Key as Relation;
                        var bRel = ent.PrimaryKey.Keys[1].Key as Relation;

                        if ((aRel == null) || (bRel == null))
                            continue;

                        if (entityList.TryGetValue(NamePostProcessor.GetTableKeyName(aRel), out EntityMap aEnt))
                        {
                            if (entityList.TryGetValue(NamePostProcessor.GetTableKeyName(bRel), out EntityMap bEnt))
                            {
                                var aRepPropName = bEnt.Name.Pluralize();
                                if (aEnt.Relations.Contains(aRepPropName))
                                    aRepPropName = $"{bEnt.Name.Pluralize()}On{ent.Name}_{aRel.ColumnName.Pascalize()}";

                                var keyName = $"{aEnt.Name}.{aRepPropName}";
                                if (entityRenames.ContainsKey(keyName))
                                    aRepPropName = entityRenames[keyName];

                                keyName = $"{aEnt.Name}.{bEnt.Name.Pluralize()}On{ent.Name}_{aRel.ColumnName.Pascalize()}";
                                if (entityRenames.ContainsKey(keyName))
                                    aRepPropName = entityRenames[keyName];

                                var aRelationProp = new Relation()
                                {
                                    IsComplexType = true,
                                    LazyLoad = true,
                                    ColumnName = aRel.ReferenceColumn ?? string.Empty,
                                    ReferenceColumn = bRel.ReferenceColumn ?? string.Empty,
                                    ReferenceProperty = bRel.ReferenceProperty ?? string.Empty,
                                    CollectionType = CollectionType.List,
                                    MapTableName = ent.TableName,
                                    MapColumn = aRel.ColumnName ?? string.Empty,
                                    MapPropertyName = aRel.ReferenceColumn ?? string.Empty,
                                    MapReferenceColumn = bRel.ColumnName ?? string.Empty,
                                    PropertyName = aRepPropName,
                                    ReferenceEntityName = bEnt.FullName,
                                    ReferenceTable = bEnt.TableName,
                                    RelationType = RelationshipType.ManyToMany,
                                    Inverse = true,
                                };
                                aEnt.Relations.Add(aRelationProp);

                                string aColCamel = aRel.ColumnName.Camelize();
                                string bColCamel = bRel.ColumnName.Camelize();

                                logger.Log(LogLevel.Debug, $"Processing map table {ent.TableName} statements -> {aColCamel}, {bColCamel}");
                                //build mapped statement for ease of adding and removing association
                                var aInsertStatement = new StatementMap
                                {
                                    Name = $"{aEnt.FullName}_{aColCamel}_associate_{aEnt.Name}_with_{bEnt.Name}",
                                    OperationType = MappedStatementType.Insert,
                                    Body = $"INSERT INTO {ent.TableName} ({aRel.ColumnName}, {bRel.ColumnName}) VALUES({StatementMap.BuildPropTag(aColCamel, aRel.ReferenceProperty)}, {StatementMap.BuildPropTag(bColCamel, bRel.ReferenceProperty)});",
                                    DependsOnEntity = aEnt.FullName
                                };
                                aInsertStatement.InputParametersMap.Add(aColCamel, aEnt.FullName);
                                aInsertStatement.InputParametersMap.Add(bColCamel, bEnt.FullName);

                                var aDeleteStatement = new StatementMap
                                {
                                    Name = $"{aEnt.FullName}_{aColCamel}_disassociate_{aEnt.Name}_with_{bEnt.Name}",
                                    OperationType = MappedStatementType.Delete,
                                    Body = string.Format("DELETE FROM {0} WHERE {1} = {3} AND {2} = {4};", ent.TableName, aRel.ColumnName, bRel.ColumnName, StatementMap.BuildPropTag(aColCamel, aRel.ReferenceProperty), StatementMap.BuildPropTag(bColCamel, bRel.ReferenceProperty)),
                                    DependsOnEntity = aEnt.FullName
                                };
                                aDeleteStatement.InputParametersMap.Add(aColCamel, aEnt.FullName);
                                aDeleteStatement.InputParametersMap.Add(bColCamel, bEnt.FullName);

                                mappedStatementStore.Add(aInsertStatement);
                                mappedStatementStore.Add(aDeleteStatement);

                                var bRepPropName = aEnt.Name.Pluralize();
                                if (bEnt.Relations.Contains(aRepPropName))
                                    bRepPropName = $"{aEnt.Name.Pluralize()}On{ent.Name}_{bRel.ColumnName.Pascalize()}";

                                keyName = $"{bEnt.Name}.{bRepPropName}";
                                if (entityRenames.ContainsKey(keyName))
                                    bRepPropName = entityRenames[keyName];

                                keyName = $"{bEnt.Name}.{aEnt.Name.Pluralize()}On{ent.Name}_{bRel.ColumnName.Pascalize()}";
                                if (entityRenames.ContainsKey(keyName))
                                    bRepPropName = entityRenames[keyName];


                                bEnt.Relations.Add(new Relation()
                                {
                                    IsComplexType = true,
                                    ColumnName = bRel.ReferenceColumn ?? string.Empty,
                                    ReferenceColumn = aRel.ReferenceColumn ?? string.Empty,
                                    ReferenceProperty = aRel.ReferenceProperty ?? string.Empty,
                                    MapTableName = ent.TableName,
                                    MapColumn = bRel.ColumnName ?? string.Empty,
                                    MapPropertyName = bRel.ReferenceColumn ?? string.Empty,
                                    MapReferenceColumn = aRel.ColumnName ?? string.Empty,
                                    CollectionType = CollectionType.List,
                                    LazyLoad = true,
                                    PropertyName = bRepPropName,
                                    ReferenceEntityName = aEnt.FullName,
                                    ReferenceTable = aEnt.TableName,
                                    ManyToManyPropertyName = aRelationProp.PropertyName,
                                    RelationType = RelationshipType.ManyToMany,
                                });
                                aRelationProp.ManyToManyPropertyName = bRepPropName;

                                var bInsertStatement = new StatementMap
                                {
                                    Name = $"{bEnt.FullName}_{bColCamel}_associate_{bEnt.Name}_with_{aEnt.Name}",
                                    OperationType = MappedStatementType.Insert,
                                    Body =
                                        $"INSERT INTO {ent.TableName} ({aRel.ColumnName}, {bRel.ColumnName}) VALUES({StatementMap.BuildPropTag(aColCamel, aRel.ReferenceProperty)}, {StatementMap.BuildPropTag(bColCamel, bRel.ReferenceProperty)});",
                                    DependsOnEntity = bEnt.FullName
                                };
                                bInsertStatement.InputParametersMap.Add(aColCamel, aEnt.FullName);
                                bInsertStatement.InputParametersMap.Add(bColCamel, bEnt.FullName);

                                var bDeleteStatement = new StatementMap
                                {
                                    Name = $"{bEnt.FullName}_{bColCamel}_dissaciate_{bEnt.Name}_with_{aEnt.Name}",
                                    OperationType = MappedStatementType.Delete,
                                    Body = string.Format("DELETE FROM {0} WHERE {1} = {3} AND {2} = {4};", ent.TableName, aRel.ColumnName, bRel.ColumnName, StatementMap.BuildPropTag(aColCamel, aRel.ReferenceProperty), StatementMap.BuildPropTag(bColCamel, bRel.ReferenceProperty)),
                                    DependsOnEntity = bEnt.FullName
                                };

                                bDeleteStatement.InputParametersMap.Add(aColCamel, aEnt.FullName);
                                bDeleteStatement.InputParametersMap.Add(bColCamel, bEnt.FullName);

                                mappedStatementStore.Add(bInsertStatement);
                                mappedStatementStore.Add(bDeleteStatement);
                            }
                        }
                    }
                    else
                    {
                        if ((ent.PrimaryKey != null) && (ent.PrimaryKey.Keys.Count == 1) && settings.SupportTableInheritance)
                        {
                            var key = ent.PrimaryKey.Keys[0];
                            if ((key != null) && (key.Key is Relation))
                            {
                                var pk = (Relation)key.Key;
                                ent.Extends = pk.ReferenceEntityName;
                                logger.Log(LogLevel.Debug, $"Processing  {ent.Name} extends -> {ent.Extends}.");
                            }
                        }
                        else if (!settings.SupportManyToMany && (ent.PrimaryKey != null) && (ent.PrimaryKey.Keys.Count > 1))
                        {
                            for (var i = 0; i < ent.PrimaryKey.Keys.Count; i++)
                            {
                                var k = ent.PrimaryKey.Keys[i].Key as Relation;
                                if (k != null && k.RelationType == RelationshipType.ManyToOne)
                                {
                                    if (entityList.TryGetValue(NamePostProcessor.GetTableKeyName(k), out EntityMap other))
                                    {
                                        logger.Log(LogLevel.Debug, $"Processing One-To-Many ent:{ent.Name} other:{other.Name}.");

                                        var aRepPropName = ent.Name.Pluralize();
                                        if (other.Relations.Contains(aRepPropName))
                                            aRepPropName = $"{ent.Name.Pluralize()}On{k.ColumnName.Pascalize()}";

                                        var keyName = $"{other.Name}.{aRepPropName}";
                                        if (entityRenames.ContainsKey(keyName))
                                            aRepPropName = entityRenames[keyName];

                                        other.Relations.Add(new Relation()
                                        {
                                            IsComplexType = true,
                                            LazyLoad = true,
                                            ColumnName = k.ReferenceColumn,
                                            ReferenceColumn = k.ColumnName,
                                            ReferenceProperty = k.PropertyName,
                                            PropertyName = aRepPropName,
                                            ReferenceTable = ent.TableName,
                                            RelationType = RelationshipType.OneToMany,
                                            ReferenceEntityName = ent.FullName,
                                            CollectionType = CollectionType.List,
                                        });
                                    }
                                }

                            }
                        }

                        for (int i = 0; i < ent.Relations.Count; i++)
                        {
                            var reference = ent.Relations[i];
                            if (reference.RelationType != RelationshipType.ManyToOne)
                            {
                                continue;
                            }

                            if (entityList.TryGetValue(NamePostProcessor.GetTableKeyName(reference), out EntityMap other))
                            {
                                logger.Log(LogLevel.Debug, $"Processing One-To-Many ent:{ent.Name} other:{other.Name}.");
                                var aRepPropName = ent.Name.Pluralize();
                                var aNameByRelationPropName = $"{ent.Name.Pluralize()}On{reference.PropertyName.Pascalize()}";
                                if (other.Relations.Contains(aRepPropName))
                                {
                                    aRepPropName = aNameByRelationPropName;
                                }

                                var keyName = $"{other.Name}.{aRepPropName}";
                                if (entityRenames.ContainsKey(keyName))
                                {
                                    aRepPropName = entityRenames[keyName];
                                }

                                keyName = $"{other.Name}.{aNameByRelationPropName}";
                                if (!aRepPropName.Equals(aNameByRelationPropName) && entityRenames.ContainsKey(keyName))
                                {
                                    aRepPropName = entityRenames[keyName];
                                }

                                other.Relations.Add(new Relation()
                                {
                                    IsComplexType = true,
                                    LazyLoad = true,
                                    ColumnName = reference.ReferenceColumn,
                                    ReferenceColumn = reference.ColumnName,
                                    ReferenceProperty = reference.PropertyName,
                                    PropertyName = aRepPropName,
                                    ReferenceTable = ent.TableName,
                                    RelationType = RelationshipType.OneToMany,
                                    ReferenceEntityName = ent.FullName,
                                    CollectionType = CollectionType.List,
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Processing exception", ex);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using Goliath.Data.Providers;

namespace Goliath.Data.Mapping
{
    class PropertiesRenameProcessor : IPostGenerationProcessor
    {
        static readonly ILogger logger;
        private SqlDialect dialect;

        static PropertiesRenameProcessor()
        {
            logger = Logger.GetLogger(typeof(RelationshipProcessor));
        }

        public PropertiesRenameProcessor(SqlDialect dialect)
        {
            this.dialect = dialect;
        }

        public void Process(IDictionary<string, EntityMap> entities, StatementStore mappedStatementStore, IDictionary<string, string> entityRenames)
        {
            if (entities == null) throw new ArgumentNullException("entities");
            if (entityRenames == null) throw new ArgumentNullException("entityRenames");

            foreach (var ent in entities.Values)
            {
                foreach (var prop in ent)
                {
                    string keyName = $"{ent.Name}.{prop.PropertyName}";
                    string renameVal;
                    if (entityRenames.TryGetValue(keyName, out renameVal))
                    {
                        var oldName = prop.PropertyName;
                        prop.PropertyName = renameVal;
                        logger.Log(LogLevel.Debug, $"Renamed {ent.Name}.{oldName} to {renameVal}");

                        var rel = prop as Relation;
                        if (rel != null)
                        {
                            EntityMap relMap = entities.Values.FirstOrDefault(c => rel.ReferenceEntityName.Equals(c.FullName));
                            if (relMap != null)
                            {
                                var bCols = relMap.Relations.Where(c => oldName.Equals(c.ReferenceProperty));
                                foreach (var relation in bCols)
                                {
                                    relation.ReferenceProperty = renameVal;
                                }
                            }
                        }
                    }
                }
            }


        }
    }
}
using System;
using System.Collections.Generic;

namespace Goliath.Data.Mapping
{
    class PropertiesRenameProcessor : IPostGenerationProcessor
    {

        public void Process(IDictionary<string, EntityMap> entities, StatementStore mappedStatementStore, IDictionary<string, string> entityRenames)
        {
            if (entities == null) throw new ArgumentNullException("entities");
            if (entityRenames == null) throw new ArgumentNullException("entityRenames");

            foreach (var ent in entities.Values)
            {
                foreach (var prop in ent)
                {
                    string keyName = string.Format("{0}.{1}", ent.Name, prop.Name);
                    string renameVal;
                    if (entityRenames.TryGetValue(keyName, out renameVal))
                    {
                        prop.PropertyName = renameVal;
                    }
                }
            }
        }
    }
}
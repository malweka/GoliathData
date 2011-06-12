using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Transformers;

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

            foreach (var table in entities.Values)
            {
                table.Name = tableNamer.Transform(table, table.Name);
                table.TableAbbreviation = tableAbbreviator.Abbreviate(table.Name);

                foreach (var rel in table.Relations)
                {
                    rel.ReferenceEntityName = tableNamer.Transform(null, rel.ReferenceTable);
                    relNamer.Transform(rel, rel.ColumnName);
                }
            }
        }

        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Mapping
{
    class UnMappedTableMap : EntityMap
    {
        private UnMappedTableMap(string entityName) : base(entityName, entityName) { }

        public static UnMappedTableMap Create(string entityName, params Property[] properties)
        {
            var table = new UnMappedTableMap(entityName);
            table.TableAlias = entityName;
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    table.Add(prop);
                }
            }
            return table;
        }
    }

}

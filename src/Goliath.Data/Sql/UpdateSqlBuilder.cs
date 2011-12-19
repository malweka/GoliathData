using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    using Diagnostics;
    using DynamicProxy;
    using Mapping;
    using Providers;
    using Sql;
    using DataAccess;

    class UpdateSqlBuilder : SqlBuilder
    {

        public UpdateSqlBuilder(SqlMapper sqlMapper, EntityMap entMap, int level, int rootLevel)
            : base(sqlMapper, entMap)
        {
            foreach (var prop in entMap)
            {
                if (prop.IsPrimaryKey || prop.IgnoreOnUpdate)
                    continue;

                string paramName = null;

                if (prop is Relation)
                {
                    var rel = (Relation)prop;
                    if (rel.RelationType != RelationshipType.ManyToOne)
                        continue;
                }

                paramName = BuildParameterNameWithLevel(prop.ColumnName, entMap.TableAlias, level);
                Columns.Add(prop.ColumnName, sqlMapper.CreateParameterName(paramName));
               
            }
        }

        public override string ToSqlString()
        {
            throw new NotImplementedException();
        }
    }
}

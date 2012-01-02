using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    using DataAccess;
    using Mapping;
    using Providers;

    class DeleteSqlBuilder : SqlBuilder
    {
        public DeleteSqlBuilder(SqlMapper sqlMapper, EntityMap entMap) : base(sqlMapper, entMap) { }
        
        public DeleteSqlBuilder Where(params WhereStatement[] whereCollection)
        {
            if (whereCollection != null)
            {
                foreach (var w in whereCollection)
                    wheres.Add(w);
            }

            return this;
        }

        public override string ToSqlString()
        {
            StringBuilder sb = new StringBuilder("DELETE FROM ");
            sb.AppendFormat("{0} WHERE ", entMap.TableName);
            foreach (var w in wheres)
            {
                sb.AppendFormat("{0} ", w.ToString());
            }
            return sb.ToString();
        }
    }
}

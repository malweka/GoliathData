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

        public static Dictionary<string, ParamHolder> BuildDeleteQueryParams(object entity, EntityGetSetInfo getSetInfo,
             EntityMap entityMap, GetSetStore getSetStore)
        {
            
            Dictionary<string, ParamHolder> parameters = new Dictionary<string, ParamHolder>();
            if (entityMap.PrimaryKey != null)
            {
                for (int i = 0; i < entityMap.PrimaryKey.Keys.Count; i++)
                {
                    PropInfo pInfo;
                    var prop = entityMap.PrimaryKey.Keys[i].Key;

                    if (getSetInfo.Properties.TryGetValue(prop.PropertyName, out pInfo))
                    {

                        string colname = entityMap.PrimaryKey.Keys[i].Key.ColumnName;
                        var paramName = BuildParameterNameWithLevel(colname, entityMap.TableAlias, 0);
                        ParamHolder param = new ParamHolder(paramName, pInfo.Getter, entity);

                        if (parameters.ContainsKey(prop.ColumnName))
                        {
                            parameters.Remove(prop.ColumnName);
                        }

                        parameters.Add(prop.ColumnName, param);
                    }
                }
            }
            return parameters;
        }

        public override string ToSqlString()
        {
            StringBuilder sb = new StringBuilder("DELETE FROM ");
            sb.AppendFormat("{0} WHERE ", entMap.TableName);
            foreach (var w in wheres)
            {
                sb.AppendFormat("{0} ", w.ToString());
            }
            return sb.ToString().Trim();
        }
    }
}

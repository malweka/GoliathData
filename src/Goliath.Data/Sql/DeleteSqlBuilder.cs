using System.Collections.Generic;
using System.Text;
using Goliath.Data.DataAccess;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    class DeleteSqlBuilder : SqlBuilder
    {
        public DeleteSqlBuilder(SqlDialect dialect, EntityMap entMap) : base(dialect, entMap) { }

        public DeleteSqlBuilder Where(params WhereStatement[] whereCollection)
        {
            if (whereCollection != null)
            {
                foreach (var w in whereCollection)
                    wheres.Add(w);
            }

            return this;
        }

        public static Dictionary<string, ParamHolder> BuildDeleteQueryParams(object entity, EntityAccessor getSetInfo,
             EntityMap entityMap, EntityAccessorStore EntityAccessorStore)
        {
            
            Dictionary<string, ParamHolder> parameters = new Dictionary<string, ParamHolder>();
            if (entityMap.PrimaryKey != null)
            {
                for (int i = 0; i < entityMap.PrimaryKey.Keys.Count; i++)
                {
                    PropertyAccessor pInfo;
                    var prop = entityMap.PrimaryKey.Keys[i].Key;

                    if (getSetInfo.Properties.TryGetValue(prop.PropertyName, out pInfo))
                    {

                        string colname = entityMap.PrimaryKey.Keys[i].Key.ColumnName;
                        var paramName = BuildParameterNameWithLevel(colname, entityMap.TableAlias, 0);
                        ParamHolder param = new ParamHolder(paramName, pInfo.GetMethod, entity);

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

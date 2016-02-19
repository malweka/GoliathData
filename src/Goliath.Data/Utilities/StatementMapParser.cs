﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using System.Linq;

namespace Goliath.Data.Utils
{
    class StatementMapParser : IMappedStatementParser
    {
        /// <summary>
        /// Parses the specified text.
        /// </summary>
        /// <param name="dialect">The mapper.</param>
        /// <param name="entMap">The ent map.</param>
        /// <param name="text">The text.</param>
        /// <param name="inputParam">The input param.</param>
        /// <param name="dbParameters">The db parameters.</param>
        /// <returns></returns>
        public CompiledStatement Parse(SqlDialect dialect, EntityMap entMap, string text, StatementInputParam inputParam, params QueryParam[] dbParameters)
        {
            text = ParseColumnTag(dialect, entMap, text);
            var statement = ParseObjectPropertyTag(dialect, entMap, inputParam, text);
            ParseDbParameters(dialect, statement, dbParameters);
            statement.Body = ParseEntityMapAllowedProperties(entMap, statement.Body);

            return statement;
        }

        /// <summary>
        /// Parses the specified text.
        /// </summary>
        /// <param name="dialect">The mapper.</param>
        /// <param name="config">The config.</param>
        /// <param name="inputParams">The input params.</param>
        /// <param name="text">The text.</param>
        /// <param name="dbParameters">The db parameters.</param>
        /// <returns></returns>
        public CompiledStatement Parse(SqlDialect dialect, MapConfig config, IDictionary<string, StatementInputParam> inputParams, string text, params QueryParam[] dbParameters)
        {
            if (inputParams == null)
            {
                inputParams = new Dictionary<string, StatementInputParam>() { };
            }

            foreach (var stat in inputParams.Values)
            {
                EntityMap entMap;
                if (!config.EntityConfigs.TryGetValue(stat.Type, out entMap))
                {
                    if (stat.ClrType == null)
                    {
                        stat.ClrType = Type.GetType(stat.Type);
                    }

                    entMap = new DynamicEntityMap(stat.ClrType);

                }
                else
                {
                    stat.IsMapped = true;
                    stat.Type = entMap.FullName;
                    if (stat.Type == null)
                        stat.ClrType = Type.GetType(entMap.FullName);
                }

                stat.Map = entMap;
            }

            if (inputParams.Count == 1)
            {
                return Parse(dialect, inputParams.Values.First().Map, text, inputParams.Values.First(), dbParameters);
            }



            text = ParseColumnTag(dialect, config, inputParams, text);
            var statement = ParseObjectPropertyTag(dialect, config, inputParams, text);
            ParseDbParameters(dialect, statement, dbParameters);
            statement.Body = ParseEntityMapAllowedProperties(config, inputParams, statement.Body);

            return statement;
        }

        void ParseDbParameters(SqlDialect dialect, CompiledStatement statement, QueryParam[] dbParameters)
        {
            if (dbParameters == null)
                dbParameters = new QueryParam[] { };

            foreach (var param in dbParameters)
            {
                statement.Body = statement.Body.Replace("@{" + param.Name + "}", dialect.CreateParameterName(param.Name));
            }

        }

        string ParseEntityMapAllowedProperties(EntityMap entMap, string text)
        {
            Dictionary<string, string> values = ExtractTags(string.Empty, text);

            foreach (var m in values)
            {
                string value = null;

                switch (m.Value)
                {
                    case "TableName":
                        value = entMap.TableName;
                        break;
                    case "Name":
                        value = entMap.Name;
                        break;
                    case "FullName":
                        value = entMap.FullName;
                        break;
                    case "Namespace":
                        value = entMap.Namespace;
                        break;
                    case "TableAlias":
                        value = entMap.TableAlias;
                        break;
                    case "SchemaName":
                        value = entMap.SchemaName;
                        break;
                    default:
                        throw new GoliathDataException(string.Format("Unrecognized tag name {0}", m.Key));
                }

                text = text.Replace(m.Key, value);

            }

            return text;
        }

        string ParseEntityMapAllowedProperties(MapConfig config, IDictionary<string, StatementInputParam> inputParams, string text)
        {
            Dictionary<string, string> values = ExtractTags(string.Empty, text);

            foreach (var m in values)
            {
                string value = null;
                var varInfo = ExtractVariables(m.Value);
                StatementInputParam inputVariable = new StatementInputParam();

                if (inputParams.TryGetValue(varInfo.VarName, out inputVariable))
                {
                    EntityMap entMap = config.GetEntityMap(inputVariable.Type);
                    switch (varInfo.PropName)
                    {
                        case "TableName":
                            value = entMap.TableName;
                            break;
                        case "Name":
                            value = entMap.Name;
                            break;
                        case "FullName":
                            value = entMap.FullName;
                            break;
                        case "Namespace":
                            value = entMap.Namespace;
                            break;
                        case "TableAlias":
                            value = entMap.TableAlias;
                            break;
                        case "SchemaName":
                            value = entMap.SchemaName;
                            break;
                        default:
                            throw new GoliathDataException(string.Format("Unrecognized tag name {0}", m.Key));
                    }

                    text = text.Replace(m.Key, value);
                }
                else
                {
                    throw new GoliathDataException(string.Format("No input parameter of name {0} defined.", varInfo.VarName));
                }
            }
            return text;
        }

        string ReplaceColumnText(SqlDialect dialect, EntityMap entMap, Property prop, string key, string text)
        {
            if (prop == null)
            {
                throw new GoliathDataException(string.Format("Tag {0} does not match any property for entity {1}", key, entMap.FullName));
            }

            string columnText = string.Empty;
            if (key.StartsWith(@"@{col:") || (entMap is DynamicEntityMap))
            {
                columnText = prop.ColumnName;
            }
            else
            {
                //var clmn = dialect.Escape(string.Format("{0}.{1}", entMap.TableAlias, prop.ColumnName));
                //TODO: regex to read content between SELECT and FROM and build TableQueryMap
                columnText = string.Format("{0}.{1} AS \"{0}.{2}\" ", entMap.TableAlias, prop.ColumnName, prop.PropertyName);

            }

            return text.Replace(key, columnText); ;
        }

        string ParseColumnTag(SqlDialect dialect, EntityMap entMap, string text)
        {
            Dictionary<string, string> values = ExtractTags("(col|sel)", 3, text);
            foreach (var m in values)
            {
                var prop = entMap[m.Value];
                text = ReplaceColumnText(dialect, entMap, prop, m.Key, text);
            }

            return text;
        }

        string ParseColumnTag(SqlDialect dialect, MapConfig config, IDictionary<string, StatementInputParam> inputParams, string text)
        {
            Dictionary<string, string> values = ExtractTags("(col|sel)", 3, text);

            foreach (var m in values)
            {
                var info = ExtractVariables(m.Value);

                StatementInputParam inputVariable = new StatementInputParam();
                if (inputParams.TryGetValue(info.VarName, out inputVariable))
                {

                    var prop = inputVariable.Map[info.PropName];
                    text = ReplaceColumnText(dialect, inputVariable.Map, prop, m.Key, text);
                }
                else
                {
                    throw new GoliathDataException(string.Format("No input parameter of name {0} defined.", info.VarName));
                }
            }

            return text;
        }

        CompiledStatement ParseObjectPropertyTag(SqlDialect dialect, EntityMap entMap, StatementInputParam inputParam, string text)
        {
            CompiledStatement statement = new CompiledStatement();
            var values = ExtractTags("prop", text);

            foreach (var m in values)
            {
                if (inputParam != null)
                {
                    var inPar = new StatementInputParam() { Type = entMap.FullName, Value = m.Value };
                    inPar.QueryParamName = m.Value.Replace('.', '_');
                    inPar.ClrType = inputParam.ClrType;
                    inPar.IsMapped = inputParam.IsMapped;
                    inPar.Name = inputParam.Name;
                    inPar.Map = inputParam.Map;
                    inPar.Property = new VarPropNameInfo() { PropName = m.Value, VarName = inputParam.Name };
                    statement.ParamPropertyMap.Add(m.Key, inPar);
                }

                text = text.Replace(m.Key, dialect.CreateParameterName(m.Value.Replace('.', '_')));
            }

            statement.Body = text;
            return statement;
        }

        CompiledStatement ParseObjectPropertyTag(SqlDialect dialect, MapConfig config, IDictionary<string, StatementInputParam> inputParams, string text)
        {
            CompiledStatement statement = new CompiledStatement();
            var values = ExtractTags("prop", text);


            if (values.Count > 0)
            {
                foreach (var m in values)
                {
                    var info = ExtractVariables(m.Value);

                    StatementInputParam inputVariable;
                    if (inputParams.TryGetValue(info.VarName, out inputVariable))
                    {
                        var inPar = new StatementInputParam();
                        inPar.QueryParamName = m.Value.Replace('.', '_');
                        inPar.ClrType = inputVariable.ClrType;
                        inPar.IsMapped = inputVariable.IsMapped;
                        inPar.Name = inputVariable.Name;
                        inPar.Map = inputVariable.Map;
                        inPar.Property = info;

                        statement.ParamPropertyMap.Add(m.Key, inPar);
                        text = text.Replace(m.Key, dialect.CreateParameterName(inPar.QueryParamName));
                    }
                }
            }

            statement.Body = text;
            return statement;
        }

        VarPropNameInfo ExtractVariables(string item)
        {
            VarPropNameInfo info = new VarPropNameInfo();

            string[] split = item.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 2)
                throw new GoliathDataException("cannot parse statement name.property missing");

            info.VarName = split[0];
            info.PropName = split[1];

            return info;
        }

        Dictionary<string, string> ExtractTags(string tagName, string text)
        {
            return ExtractTags(tagName, tagName.Length, text);
        }

        Dictionary<string, string> ExtractTags(string tagName, int tagLength, string text)
        {
            //int tagLength;
            if (!string.IsNullOrEmpty(tagName))
            {
                tagName = tagName + @"\:";
                tagLength = tagLength + 3;
            }
            else
                tagLength = tagLength + 2;

            string tagPattern = @"(@\{" + tagName + @"(.*?)\})";


            var matches = Regex.Matches(text, tagPattern);
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (Match m in matches)
            {
                string matchValue = m.Value;
                if (!values.ContainsKey(matchValue))
                {
                    string propName = matchValue.Substring(tagLength, matchValue.Length - (tagLength + 1));
                    values.Add(matchValue, propName);
                }
            }

            return values;
        }
    }

    public interface IMappedStatementParser
    {
        /// <summary>
        /// Parses the specified text.
        /// </summary>
        /// <param name="dialect">The mapper.</param>
        /// <param name="entMap">The ent map.</param>
        /// <param name="text">The text.</param>
        /// <param name="inputParam">The input param.</param>
        /// <param name="dbParameters">The db parameters.</param>
        /// <returns></returns>
        CompiledStatement Parse(SqlDialect dialect, EntityMap entMap, string text, StatementInputParam inputParam, params QueryParam[] dbParameters);

        /// <summary>
        /// Parses the specified text.
        /// </summary>
        /// <param name="dialect">The mapper.</param>
        /// <param name="config">The config.</param>
        /// <param name="inputParams">The input params.</param>
        /// <param name="text">The text.</param>
        /// <param name="dbParameters">The db parameters.</param>
        /// <returns></returns>
        CompiledStatement Parse(SqlDialect dialect, MapConfig config, IDictionary<string, StatementInputParam> inputParams, string text, params QueryParam[] dbParameters);
    }

}

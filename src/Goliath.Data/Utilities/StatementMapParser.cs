using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Goliath.Data.Utils
{
    using Mapping;
    using Providers;

    class StatementMapParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatementMapParser"/> class.
        /// </summary>
        public StatementMapParser() { }

        /// <summary>
        /// Parses the specified text.
        /// </summary>
        /// <param name="entMap">The ent map.</param>
        /// <param name="text">The text.</param>
        public CompiledStatement Parse(SqlMapper mapper, EntityMap entMap, string text)
        {
            text = ParseColumnTag(entMap, text);
            var statement = ParseObjectPropertyTag(mapper, entMap, text);
            statement.Body =  ParseEntityMapAllowedProperties(entMap, statement.Body);
            return statement;
        }

        /// <summary>
        /// Parses the specified text.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="config">The config.</param>
        /// <param name="inputParams">The input params.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public CompiledStatement Parse(SqlMapper mapper, MapConfig config, IDictionary<string, StatemenInputParam> inputParams, string text)
        {
            text = ParseColumnTag(config, inputParams, text);
            var statement = ParseObjectPropertyTag(mapper, config, inputParams, text);
            statement.Body = ParseEntityMapAllowedProperties(config, inputParams, statement.Body);
            return statement;
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

        string ParseEntityMapAllowedProperties(MapConfig config, IDictionary<string, StatemenInputParam> inputParams, string text)
        {
            Dictionary<string, string> values = ExtractTags(string.Empty, text);
            
            foreach (var m in values)
            {
                string value = null;
                var varInfo = ExtractVariables(m.Value);
                 StatemenInputParam inputVariable = new StatemenInputParam();

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

        string ParseColumnTag(EntityMap entMap, string text)
        {
            Dictionary<string, string> values = ExtractTags("(col|sel)", 3, text);
            foreach (var m in values)
            {
                var prop = entMap[m.Value];
                if (prop == null)
                {
                    throw new GoliathDataException(string.Format("Tag {0} does not match any property for entity {1}", m.Key, entMap.FullName));
                }

                string columnText = string.Empty;
                if (m.Key.StartsWith(@"@{sel:"))
                {
                    columnText = string.Format("{0} as {1}", prop.ColumnName, ParameterNameBuilderHelper.ColumnQueryName(prop.ColumnName, entMap.TableAlias));
                }
                else
                {
                    columnText = prop.ColumnName;
                }

                text = text.Replace(m.Key, columnText);
            }

            return text;
        }

        string ParseColumnTag(MapConfig config, IDictionary<string, StatemenInputParam> inputParams, string text)
        {
            Dictionary<string, string> values = ExtractTags("(col|sel)", 3, text);
            foreach (var m in values)
            {
                var info = ExtractVariables(m.Value);

                StatemenInputParam inputVariable = new StatemenInputParam();

                if (inputParams.TryGetValue(info.VarName, out inputVariable))
                {
                    EntityMap entMap = config.GetEntityMap(inputVariable.Type);
                    var prop = entMap[info.PropName];

                    if (prop == null)
                    {
                        throw new GoliathDataException(string.Format("Tag {0} does not match any property for entity {1}", m.Key, entMap.FullName));
                    }

                    string columnText = string.Empty;
                    if (m.Key.StartsWith(@"@{sel:"))
                    {
                        columnText = string.Format("{0} as {1}", prop.ColumnName, ParameterNameBuilderHelper.ColumnQueryName(prop.ColumnName, entMap.TableAlias));
                    }
                    else
                    {
                        columnText = prop.ColumnName;
                    }

                    text = text.Replace(m.Key, columnText);
                }
                else
                {
                    throw new GoliathDataException(string.Format("No input parameter of name {0} defined.", info.VarName));
                }
            }

            return text;
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

        struct VarPropNameInfo
        {
            public string VarName;
            public string PropName;
        }

        CompiledStatement ParseObjectPropertyTag(SqlMapper mapper, EntityMap entMap, string text)
        {
            CompiledStatement statement = new CompiledStatement();
            var values = ExtractTags("prop", text);

            foreach (var m in values)
            {
                statement.ParamPropertyMap.Add(m.Key, new StatemenInputParam() { Name = m.Key, Type = entMap.FullName, Value = m.Value });
                text = text.Replace(m.Key, mapper.CreateParameterName(m.Value.Replace('.', '_')));

            }

            statement.Body = text;
            return statement;
        }

        CompiledStatement ParseObjectPropertyTag(SqlMapper mapper, MapConfig config, IDictionary<string, StatemenInputParam> inputParams, string text)
        {
            CompiledStatement statement = new CompiledStatement();
            var values = ExtractTags("prop", text);

            foreach (var m in values)
            {
                var info = ExtractVariables(m.Value);

                StatemenInputParam inputVariable = new StatemenInputParam();

                if (inputParams.TryGetValue(info.VarName, out inputVariable))
                {
                    EntityMap entMap;
                    if (config.EntityConfigs.TryGetValue(inputVariable.Type, out entMap))
                    {
                        inputVariable.Type = entMap.FullName;
                        inputVariable.IsMapped = true;
                    }

                    statement.ParamPropertyMap.Add(m.Key, inputVariable);
                    text = text.Replace(m.Key, mapper.CreateParameterName(m.Value.Replace('.', '_')));
                }
            }

            statement.Body = text;
            return statement;
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
                    string propName = matchValue.Substring(tagLength, matchValue.Length - (tagLength+1));
                    values.Add(matchValue, propName);
                }
            }

            return values;
        }

    }
}

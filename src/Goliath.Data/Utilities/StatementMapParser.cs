using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;
using System.Reflection;

namespace Goliath.Data.Utils
{
    using Mapping;
    using DataAccess;
    using Providers;

    class StatementMapParser
    {
        public const string TAG_PATTERN = @"(@\{\w+\})";
        public const string PROP_TAG_PATTERN = @"(@\{prop\:\w+\})";
        public const string COL_TAG_PATTERN = @"(@\{col\:\w+\})";

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
            var matches = Regex.Matches(text, COL_TAG_PATTERN);
            text = ParseColTags(entMap, text);
            var statement = ParsePropTags(mapper, entMap, text);
            statement.Body =  ParseProps(entMap, statement.Body);
            return statement;
        }

        string ParseProps(EntityMap entMap, string text)
        {
            var matches = Regex.Matches(text, TAG_PATTERN);
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (Match m in matches)
            {
                string matchValue = m.Value;
                if (!values.ContainsKey(matchValue))
                {
                    string propName = matchValue.Substring(2, matchValue.Length - 3);
                    string value = string.Empty;

                    switch(propName)
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
                            throw new GoliathDataException(string.Format("Unrecognized tag name {0}", matchValue));
                    }

                    text = text.Replace(matchValue, value);
                    values.Add(matchValue, value);
                }
            }
            return text;
        }

        string ParseColTags(EntityMap entMap, string text)
        {
            var matches = Regex.Matches(text, COL_TAG_PATTERN);
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (Match m in matches)
            {
                string matchValue = m.Value;
                if (!values.ContainsKey(matchValue))
                {
                    string propName = matchValue.Substring(6, matchValue.Length - 7); //7: 6 character prefix + 1 charater ending tag.
                    var prop = entMap[propName];
                    if (prop == null)
                    {
                        throw new GoliathDataException(string.Format("Tag {0} does not match any property for entity {1}", matchValue, entMap.FullName));
                    }

                    text = text.Replace(matchValue, prop.ColumnName);
                    values.Add(matchValue, propName);
                }
            }
            return text;
        }

        CompiledStatement ParsePropTags(SqlMapper mapper, EntityMap entMap, string text)
        {
            var matches = Regex.Matches(text, PROP_TAG_PATTERN);
            CompiledStatement statement = new CompiledStatement();

            foreach (Match m in matches)
            {
                string matchValue = m.Value;
                if (!statement.ParamPropertyMap.ContainsKey(matchValue))
                {
                    string propName = matchValue.Substring(7, matchValue.Length - 8); //8: 7 character prefix + 1 charater ending tag.
                    //var prop = entMap[propName];
                    //if (prop == null)
                    //{
                    //    throw new GoliathDataException(string.Format("Tag {0} does not match any property for entity {1}", matchValue, entMap.FullName));
                    //}
                    text = text.Replace(matchValue, mapper.CreateParameterName(propName.Replace('.', '_')));
                    statement.ParamPropertyMap.Add(matchValue, propName);
                }
            }
            statement.Body = text;
            return statement;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Goliath.Data.Diagnostics;
using Goliath.Data.Providers;
using Goliath.Data.Transformers;
using Goliath.Data.Utils;
using Microsoft.SqlServer.Server;

namespace Goliath.Data.Mapping
{
    class NamePostProcessor : IPostGenerationProcessor
    {
        NameTransformerFactory transfactory;
        ITableNameAbbreviator tableAbbreviator;
        private SqlDialect dialect;
        private ILogger logger = Logger.GetLogger(typeof(NamePostProcessor));

        public NamePostProcessor(SqlDialect dialect, NameTransformerFactory transformerFactory, ITableNameAbbreviator tableAbbreviator)
        {
            if (transformerFactory == null)
                throw new ArgumentNullException("transformerFactory");
            if (tableAbbreviator == null)
                throw new ArgumentNullException("tableAbbreviator");

            transfactory = transformerFactory;
            this.tableAbbreviator = tableAbbreviator;
            this.dialect = dialect;
        }

        private static readonly Regex rxCleanUp = new Regex(@"[^\w\d_]", RegexOptions.Compiled);

        internal static string CleanUpString(string str)
        {
            // Replace punctuation and symbols in variable names as these are not allowed.
            int len = str.Length;
            if (len == 0)
                return str;
            var sb = new StringBuilder();
            bool replacedCharacter = false;
            for (int n = 0; n < len; ++n)
            {
                char c = str[n];
                if (c != '_' && (char.IsSymbol(c) || char.IsPunctuation(c)))
                {
                    int ascii = c;
                    sb.AppendFormat("{0}", ascii);
                    replacedCharacter = true;
                    continue;
                }
                sb.Append(c);
            }
            if (replacedCharacter)
                str = sb.ToString();

            // Remove non alphanumerics
            str = rxCleanUp.Replace(str, "");
            if (char.IsDigit(str[0]))
                str = "C" + str;

            return str;
        }

        #region IPostGenerationProcessor Members

        public void Process(IDictionary<string, EntityMap> entities, 
            StatementStore mappedStatementStore, 
            IDictionary<string, string> entityRenames)
        {
            ProcessTableNames(entities, entityRenames);
        }

        #endregion

        public static string GetTableKeyName(Relation rel)
        {
            //if (!string.IsNullOrWhiteSpace(rel.ReferenceTableSchemaName) && rel.ReferenceTableSchemaName.ToUpper()
            //        .Equals(dialect.DefaultSchemaName.ToUpper()))
            //    return rel.ReferenceTable;

            return $"{rel.ReferenceTableSchemaName}.{rel.ReferenceTable}";
        }

        void ProcessTableNames(IDictionary<string, EntityMap> entities, 
            IDictionary<string, string> entityRenames)
        {
            var tableNamer = transfactory.GetTransformer<EntityMap>();
            var relNamer = transfactory.GetTransformer<Relation>();
            var propNamer = transfactory.GetTransformer<Property>();

            foreach (var table in entities.Values)
            {
                table.Name = GetRename(tableNamer.Transform(table, table.Name), entityRenames);
                table.TableAlias = tableAbbreviator.Abbreviate(table.Name).ToLower();
                var propertyListClone = table.ToArray();
                logger.Log(LogLevel.Info, $"Processing table {table.SchemaName}.{table.TableName}");
                foreach (var prop in propertyListClone)
                {
                    if (prop is Relation)
                    {
                        var rel = (Relation)prop;

                        EntityMap refEnt;
                        var refTblKey = GetTableKeyName(rel);
                        if (entities.TryGetValue(refTblKey, out refEnt))
                        {
                            rel.ReferenceEntityName = string.Format("{0}.{1}", refEnt.Namespace, GetRename(tableNamer.Transform(null, rel.ReferenceTable), entityRenames));
                        }

                        string name = relNamer.Transform(rel, rel.ColumnName);
                        if (!string.IsNullOrWhiteSpace(rel.MapPropertyName))
                        {
                            string mapPropName = propNamer.Transform(rel, rel.MapPropertyName);
                            rel.MapPropertyName = mapPropName;
                        }

                        if (rel.RelationType == RelationshipType.ManyToOne)
                        {
                            Property newProperty = rel.Clone();
                            if (rel.IsPrimaryKey)
                            {
                                newProperty.IsPrimaryKey = false;
                            }
                            newProperty.PropertyName = propNamer.Transform(rel, rel.ColumnName);
                            table.Remove(rel);
                            rel.PropertyName = name;

                            if (table.ContainsProperty(name))
                            {
                                var pcount = table.Properties.Count(p =>
                                                 p.PropertyName.StartsWith(newProperty.PropertyName))
                                             + table.Relations.Count(p =>
                                                 p.PropertyName.StartsWith(newProperty.PropertyName));
                                rel.PropertyName = $"{name}{pcount + 1}";
                            }

                            if (rel.PropertyName.Equals(newProperty.PropertyName))
                            {
                                rel.PropertyName = string.Concat(name, "Entity");
                            }

                            logger.Log(LogLevel.Debug, $"\tRelationship {rel.PropertyName} | {newProperty.ColumnName} <-> {newProperty.PropertyName}");
                            table.Add(rel);
                            table.Add(newProperty);
                        }

                        if (!string.IsNullOrWhiteSpace(rel.ReferenceProperty))
                        {
                            rel.ReferenceProperty = propNamer.Transform(rel, rel.ReferenceProperty);
                        }
                    }
                    else
                    {
                        prop.PropertyName = propNamer.Transform(prop, prop.ColumnName);
                    }

                    if (prop.PropertyName.Equals(table.Name))
                    {
                        //member name cannot be the same as enclosing type. We rename
                        prop.PropertyName = prop.PropertyName + "Property";
                    }
                }

                if (!table.IsLinkTable && table.PrimaryKey != null)
                {
                    foreach (var pk in table.PrimaryKey.Keys)
                    {
                        if (pk.Key.DbType == System.Data.DbType.Guid)
                        {
                            pk.KeyGenerationStrategy = Generators.GuidCombGenerator.GeneratorName;
                            pk.UnsavedValueString = Guid.Empty.ToString();
                        }
                        else if (pk.Key.IsIdentity)
                        {
                            pk.KeyGenerationStrategy = Generators.AutoIncrementGenerator.GeneratorName;
                            pk.UnsavedValueString = "0";
                        }
                    }
                }

            }
        }


        string GetRename(string tableName, IDictionary<string, string> entityRenames)
        {
            if (entityRenames == null) return tableName;

            string newName;
            if (entityRenames.TryGetValue(tableName, out newName))
                return newName;

            return tableName;
        }

    }
}

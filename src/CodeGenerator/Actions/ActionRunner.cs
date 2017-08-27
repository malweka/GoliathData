using System;
using System.Collections.Generic;
using System.IO;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;

namespace Goliath.Data.CodeGenerator.Actions
{
    abstract class ActionRunner : IActionRunner
    {
        protected static ILogger logger;
        protected const string prima_orda_file = "prima_orda.log";

        static ActionRunner()
        {
            logger = Logger.GetLogger(typeof(ActionRunner));
        }

        protected ActionRunner(string actionName)
        {
            ActionName = actionName;
        }

        public string ActionName { get; }

        public abstract void Exetute(AppOptionInfo opts, CodeGenRunner codeGenRunner);

        protected static string GetCodeMapFile(AppOptionInfo opts, bool throwIfNotExist = true)
        {
            string codeMapFile = opts.MapFile;

            if (!Path.IsPathRooted(codeMapFile))
            {
                codeMapFile = Path.GetFullPath(codeMapFile);
            }

            Console.WriteLine("Reading Code Map file...");
            if (!File.Exists(codeMapFile) && throwIfNotExist)
                throw new GoliathDataException($"Map file {codeMapFile} not found.");

            return codeMapFile;
        }

        protected static string GetFileName(string entityName, int? iteration, string outputFile)
        {
            string fileName;
            if (outputFile.Contains("(name)") || outputFile.Contains("(iteration)"))
            {
                fileName = outputFile.Replace("(name)", entityName);
                if (iteration.HasValue)
                    fileName = fileName.Replace("(iteration)", $"{iteration.Value:D3}");
            }
            else
            {
                fileName = string.Concat(entityName, outputFile);
            }

            return fileName;
        }

        protected static SupportedRdbms GetRdbms(string rdbmsName)
        {
            var rdbms = SupportedRdbms.Mssql2008R2;

            if (string.IsNullOrWhiteSpace(rdbmsName)) return rdbms;

            switch (rdbmsName.ToUpper())
            {
                case "MSSQL2008":
                    rdbms = SupportedRdbms.Mssql2008;
                    break;
                case "MSSQL2008R2":
                    rdbms = SupportedRdbms.Mssql2008R2;
                    break;
                case "POSTGRESQL8":
                    rdbms = SupportedRdbms.Postgresql8;
                    break;
                case "POSTGRESQL9":
                    rdbms = SupportedRdbms.Postgresql9;
                    break;
                case "SQLITE3":
                    rdbms = SupportedRdbms.Sqlite3;
                    break;
                default:
                    rdbms = SupportedRdbms.Mssql2008R2;
                    break;
            }

            return rdbms;
        }

        protected static SupportedRdbms GetSupportedRdbms(AppOptionInfo opts)
        {
            SupportedRdbms rdbms;

            if (!string.IsNullOrWhiteSpace(opts.ProviderName))
            {
                switch (opts.ProviderName.ToUpper())
                {
                    case "MSSQL2008":
                        rdbms = SupportedRdbms.Mssql2008;
                        break;
                    case "MSSQL2008R2":
                        rdbms = SupportedRdbms.Mssql2008R2;
                        break;
                    case "POSTGRESQL8":
                        rdbms = SupportedRdbms.Postgresql8;
                        break;
                    case "POSTGRESQL9":
                        rdbms = SupportedRdbms.Postgresql9;
                        break;
                    case "SQLITE3":
                        rdbms = SupportedRdbms.Sqlite3;
                        break;
                    default:
                        rdbms = SupportedRdbms.Mssql2008;
                        break;
                }
            }

            else rdbms = SupportedRdbms.Mssql2008R2;
            return rdbms;
        }

        protected static void ProcessMetadata(AppOptionInfo opts, EntityMap ent)
        {
            List<Tuple<string, string>> metadata;
            if (opts.MetadataDictionary.TryGetValue(ent.Name, out metadata))
            {
                foreach (var tuple in metadata)
                {
                    ent.MetaDataAttributes.Add(tuple.Item1.Replace("data_", string.Empty), tuple.Item2);
                }
            }
        }

        protected static void ProcessMetadata(AppOptionInfo opts, EntityMap ent, Property prop)
        {
            var key = string.Concat(ent.Name, ".", prop.Name);
            List<Tuple<string, string>> metadata;
            if (opts.MetadataDictionary.TryGetValue(key, out metadata))
            {
                foreach (var tuple in metadata)
                {
                    prop.MetaDataAttributes.Add(tuple.Item1.Replace("data_", string.Empty), tuple.Item2);
                }
            }

            if (prop.PropertyName.Equals("CreatedOn") || prop.PropertyName.Equals("CreatedBy"))
            {
                prop.MetaDataAttributes.Add("editable", "false");
            }
        }

        protected static void ProcessActivatedProperties(AppOptionInfo opts, EntityMap ent)
        {
            var isTrackable = string.Concat(ent.Name, ".IsTrackable");
            var extends = string.Concat(ent.Name, ".Extends");
            var tableAlias = string.Concat(ent.Name, ".TableAlias");

            if (opts.ActivatedProperties.ContainsKey(isTrackable))
            {
                bool val;
                bool.TryParse(opts.ActivatedProperties[isTrackable], out val);
                ent.IsTrackable = val;
            }

            if (opts.ActivatedProperties.ContainsKey(extends))
            {
                ent.Extends = opts.ActivatedProperties[extends];
            }

            if (opts.ActivatedProperties.ContainsKey(tableAlias))
            {
                ent.TableAlias = opts.ActivatedProperties[tableAlias];
            }
        }

        protected static void ProcessActivatedProperties(AppOptionInfo opts, EntityMap ent, Property prop)
        {
            var ignoreOnUpdate = string.Concat(ent.Name, ".", prop.Name, ".IgnoreOnUpdate");
            var lazyload = string.Concat(ent.Name, ".", prop.Name, ".LazyLoad");
            var isNullable = string.Concat(ent.Name, ".", prop.Name, ".IsNullable");
            var isUnique = string.Concat(ent.Name, ".", prop.Name, ".IsUnique");

            if (opts.ActivatedProperties.ContainsKey(ignoreOnUpdate))
            {
                bool val;
                bool.TryParse(opts.ActivatedProperties[ignoreOnUpdate], out val);
                prop.IgnoreOnUpdate = val;
            }

            if (opts.ActivatedProperties.ContainsKey(lazyload))
            {
                bool val;
                bool.TryParse(opts.ActivatedProperties[lazyload], out val);
                prop.LazyLoad = val;
            }

            if (opts.ActivatedProperties.ContainsKey(isNullable))
            {
                bool val;
                bool.TryParse(opts.ActivatedProperties[isNullable], out val);
                prop.IsNullable = val;
            }

            if (opts.ActivatedProperties.ContainsKey(isUnique))
            {
                bool val;
                bool.TryParse(opts.ActivatedProperties[isUnique], out val);
                prop.IsUnique = val;
            }

            if (prop.PropertyName.Equals("CreatedOn") || prop.PropertyName.Equals("CreatedBy"))
            {
                prop.IgnoreOnUpdate = true;
            }
        }
    }
}
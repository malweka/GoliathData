using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.CodeGenerator
{
    class AppOptionHandler
    {
        static readonly ILogger logger;

        static AppOptionHandler()
        {
            logger = Logger.GetLogger(typeof(AppOptionHandler));
        }

        public static AppOptionInfo ParseOptions(string[] args)
        {
            var opts = new AppOptionInfo();
            var p = new OptionSet()
                .Add("action=", c =>
                               {
                                   opts.ActionName = c;
                                   Console.WriteLine(c);
                               })
                .Add("includeGeneratedColumns=", c =>
                {
                    bool includeGeneratedColumns;
                    if (bool.TryParse(c, out includeGeneratedColumns))
                        opts.ExportDatabaseGeneratedColumns = includeGeneratedColumns;
                })
                .Add("includeIdentityColumn=", c =>
                {
                    bool includeIdentityColumn;
                    if (bool.TryParse(c, out includeIdentityColumn))
                        opts.ExportIdentityColumn = includeIdentityColumn;
                })

                .Add("createMap", w => opts.ActionName = w)
                .Add("combineMaps", w => opts.ActionName = w)
                .Add("export", w => opts.ActionName = w)
                .Add("generate", w => opts.ActionName = w)
                .Add("generateAll", w => opts.ActionName = w)
                .Add("generateEnum", w => opts.ActionName = w)
                .Add("generateEntities", w => opts.ActionName = w)
                .Add("namespace=|n=", w => opts.Namespace = w)
                .Add("baseModel=", w => opts.BaseModelXml = w)
                .Add("exclude=", w => opts.Excluded = w)
                .Add("include=", w => opts.Include = w)
                .Add("entity=", w => opts.EntityModel = w)
                .Add("renameConfig=", w => opts.RenameConfig = w)
                .Add("complexTypeMap=", w => opts.ComplexTypeMap = w)
                .Add("extension=", w => opts.ExtensionMap = w)
                .Add("defaultKeygen=", w => opts.DefaultKeygen = w)
                .Add("statementMap=", w => opts.MappedStatementFile = w)
                .Add("datamap=|map=|m=", w => opts.MapFile = w)
                .Add("connectionstring=|c=", w => opts.ConnectionString = w)
                .Add("provider=", w => opts.ProviderName = w)
                .Add("assembly=|a=", w => opts.AssemblyName = w)
                .Add("workingFolder=|w=", w => opts.WorkingFolder = w)
                .Add("out=|o=", w => opts.OutputFile = w)
                .Add("in=|i=", w => opts.TemplateName = w)
                .Add("queryProvider=", w => opts.QueryProviderName = w)
                .Add("importDialect=", w => opts.ImportSqlDialect = w)
                .Add("exportDialect=", w => opts.ExportSqlDialect = w)
                .Add("templateFolder=|t=", w => opts.TemplateFolder = w);

            p.Parse(args);

            if (string.IsNullOrWhiteSpace(opts.ConnectionString))
            {
                string providerName;
                opts.ConnectionString = ExtractConnectionString("default", out providerName);
                opts.ProviderName = providerName;
            }

            if (string.IsNullOrWhiteSpace(opts.Namespace))
                opts.Namespace = "MyApp.Name.Core";

            if (string.IsNullOrWhiteSpace(opts.AssemblyName))
                opts.AssemblyName = opts.Namespace;

            if (string.IsNullOrWhiteSpace(opts.ProviderName))
                opts.ProviderName = "MSSQL2008";

            if (string.IsNullOrWhiteSpace(opts.MapFile))
                opts.MapFile = "data.map.xml";

            if (string.IsNullOrWhiteSpace(opts.WorkingFolder))
            {
                opts.WorkingFolder = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                var isRooted = Path.IsPathRooted(opts.WorkingFolder);
                if (!isRooted)
                    opts.WorkingFolder = Path.GetFullPath(opts.WorkingFolder);
            }

            if (string.IsNullOrWhiteSpace(opts.TemplateFolder))
                opts.TemplateFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");

            if (string.IsNullOrWhiteSpace(opts.ActionName))
                opts.ActionName = "createMap";

            if (string.IsNullOrWhiteSpace(opts.Excluded))
                opts.ExcludedArray = new string[] { };
            else
                opts.ExcludedArray = opts.Excluded.Split(new string[] { ",", ";", "|" }, StringSplitOptions.RemoveEmptyEntries);

            ProcessRenames(opts);
            ProcessCompleTypeMap(opts);
            ProcessExtensionMap(opts);

            return opts;
        }

        static string ExtractConnectionString(string cnName, out string providerName)
        {
            providerName = string.Empty;
            var cnn = System.Configuration.ConfigurationManager.ConnectionStrings[cnName];
            if (cnn != null)
                return cnn.ConnectionString;
            else return null;
        }

        private static void ProcessRenames(AppOptionInfo opts)
        {
            if (string.IsNullOrWhiteSpace(opts.RenameConfig)) return;

            if (!File.Exists(opts.RenameConfig)) return;
            try
            {
                using (var reader = new StreamReader(opts.RenameConfig))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var names = line.Split(new string[] { ",", ";", "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (names.Length > 1)
                        {
                            if (!opts.EntitiesToRename.ContainsKey(names[0]))
                                opts.EntitiesToRename.Add(names[0], names[1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogException("Could not process rename config file.", ex);
            }
        }

        private static void ProcessCompleTypeMap(AppOptionInfo opts)
        {
            if (string.IsNullOrWhiteSpace(opts.ComplexTypeMap)) return;

            if (!File.Exists(opts.ComplexTypeMap)) return;
            try
            {
                using (var reader = new StreamReader(opts.ComplexTypeMap))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var names = line.Split(new string[] { ",", ";", "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (names.Length > 1)
                        {
                            if (!opts.ComplexTypesTypeMap.ContainsKey(names[0]))
                                opts.ComplexTypesTypeMap.Add(names[0], names[1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogException("Could not process complex type map file.", ex);
            }
        }

        private static void ProcessExtensionMap(AppOptionInfo opts)
        {
            if (string.IsNullOrWhiteSpace(opts.ExtensionMap)) return;

            if (!File.Exists(opts.ExtensionMap)) return;
            try
            {
                using (var reader = new StreamReader(opts.ExtensionMap))
                {
                    string txt = reader.ReadToEnd();
                    var lines = txt.Split(new string[] { "\n", "\r", ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var stms = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (stms.Length > 1)
                        {
                            var name = stms[0].Trim();
                            var value = stms[1].Trim().Replace("\"", string.Empty).Replace("'", string.Empty);

                            var entParts = name.Trim().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                            if (name.Contains("data_"))
                            {
                                if (entParts.Length > 2)
                                {
                                    var key = string.Concat(entParts[0].Trim(), ".", entParts[1].Trim());
                                    if (!opts.MetadataDictionary.ContainsKey(key))
                                    {
                                        var lst = new List<Tuple<string, string>> { Tuple.Create(entParts[2].Trim(), value) };
                                        opts.MetadataDictionary.Add(key, lst);
                                    }
                                    else
                                    {
                                        opts.MetadataDictionary[key].Add(Tuple.Create(entParts[2].Trim(), value));
                                    }
                                }
                                else
                                {
                                    var key = entParts[0].Trim();
                                    if (!opts.MetadataDictionary.ContainsKey(key))
                                    {
                                        var lst = new List<Tuple<string, string>> { Tuple.Create(entParts[1].Trim(), value) };
                                        opts.MetadataDictionary.Add(key, lst);
                                    }
                                    else
                                    {
                                        opts.MetadataDictionary[key].Add(Tuple.Create(entParts[1].Trim(), value));
                                    }
                                }
                            }
                            else
                            {
                                if (!opts.ActivatedProperties.ContainsKey(name))
                                    opts.ActivatedProperties.Add(name, value);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogException("Could not process complex type map file.", ex);
            }
        }

    }


    class AppOptionInfo
    {
        readonly Dictionary<string, string> entitiesToRename = new Dictionary<string, string>();
        readonly Dictionary<string, List<Tuple<string, string>>> metadataDictionary = new Dictionary<string, List<Tuple<string, string>>>();
        public Dictionary<string, string> EntitiesToRename { get { return entitiesToRename; } }
        private readonly Dictionary<string, string> complexTypesTypeMap = new Dictionary<string, string>();
        public Dictionary<string, string> ComplexTypesTypeMap { get { return complexTypesTypeMap; } }
        public Dictionary<string, List<Tuple<string, string>>> MetadataDictionary { get { return metadataDictionary; } }
        readonly Dictionary<string, string> properties = new Dictionary<string, string>();

        public Dictionary<string, string> ActivatedProperties { get { return properties; } }

        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }
        public string QueryProviderName { get; set; }
        public string WorkingFolder { get; set; }
        public string TemplateFolder { get; set; }
        public string Namespace { get; set; }
        public string AssemblyName { get; set; }
        public string MapFile { get; set; }
        public string TemplateName { get; set; }
        public string OutputFile { get; set; }
        public string ActionName { get; set; }
        public string BaseModelXml { get; set; }
        public string Excluded { get; set; }
        public string Include { get; set; }
        public string RenameConfig { get; set; }
        public string EntityModel { get; set; }
        public string MappedStatementFile { get; set; }
        public string DefaultKeygen { get; set; }
        public string ComplexTypeMap { get; set; }
        public string ExtensionMap { get; set; }
        public bool ExportDatabaseGeneratedColumns { get; set; }
        public bool ExportIdentityColumn { get; set; }
        public string ImportSqlDialect { get; set; }
        public string ExportSqlDialect { get; set; }
        public string[] ExcludedArray { get; set; }
    }
}
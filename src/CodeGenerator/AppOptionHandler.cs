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

                .Add("createMap", w => opts.ActionName = w)
                .Add("generate", w => opts.ActionName = w)
                .Add("generateEntities", w => opts.ActionName = w)
                .Add("namespace=|n=", w => opts.Namespace = w)
                .Add("baseModel=", w => opts.BaseModelXml = w)
                .Add("exclude=", w => opts.Excluded = w)
                .Add("entity=", w => opts.EntityModel = w)
                .Add("renameConfig=", w => opts.RenameConfig = w)
                .Add("datamap=|map=|m=", w => opts.MapFile = w)
                .Add("connectionstring=|c=", w => opts.ConnectionString = w)
                .Add("provider=", w => opts.ProviderName = w)
                .Add("assembly=|a=", w => opts.AssemblyName = w)
                .Add("workingFolder=|w=", w => opts.WorkingFolder = w)
                .Add("out=|o=", w => opts.OutputFile = w)
                .Add("in=|i=", w => opts.TemplateName = w)
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
                opts.WorkingFolder = AppDomain.CurrentDomain.BaseDirectory;

            if (string.IsNullOrWhiteSpace(opts.TemplateFolder))
                opts.TemplateFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");

            if (string.IsNullOrWhiteSpace(opts.ActionName))
                opts.ActionName = "createMap";

            if (string.IsNullOrWhiteSpace(opts.Excluded))
                opts.ExcludedArray = new string[] { };
            else
                opts.ExcludedArray = opts.Excluded.Split(new string[] { ",", ";", "|" }, StringSplitOptions.RemoveEmptyEntries);

            ProcessRenames(opts);

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

    }


    class AppOptionInfo
    {
        readonly Dictionary<string, string> entitiesToRename = new Dictionary<string, string>();
        public Dictionary<string, string> EntitiesToRename { get { return entitiesToRename; } }
        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }
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
        public string RenameConfig { get; set; }
        public string EntityModel { get; set; }

        public string[] ExcludedArray { get; set; }
    }
}
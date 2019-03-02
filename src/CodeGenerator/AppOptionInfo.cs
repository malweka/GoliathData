using System;
using System.Collections.Generic;

namespace Goliath.Data.CodeGenerator
{
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
        public bool Compress { get; set; }
        public bool SupportManyToMany { get; set; } = true;
        public string FileSizeLimitInKb { get; set; }


    }
}
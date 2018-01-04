using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Goliath.Data.DataAccess;
using Goliath.Data.Mapping;

namespace Goliath.Data.CodeGenerator.Actions
{
    class ExportAction : ActionRunner
    {
        public const string Name = "EXPORT";
        public ExportAction() : base(Name)
        {
        }

        public override void Exetute(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            var codeMapFile = GetCodeMapFile(opts);
            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;

            var providerFactory = new ProviderFactory();

            var importRdbms = GetRdbms(opts.ImportSqlDialect);
            var exportRdbms = GetRdbms(opts.ExportSqlDialect);

            var dbConnector = providerFactory.CreateDbConnector(importRdbms, opts.ConnectionString);

            if (string.IsNullOrWhiteSpace(opts.TemplateName))
                throw new GoliathDataException("Template file to use is required for generate operation. Please make sure that -in=\"Template_File_name.razt\" argument is passed in.");

            var template = Path.Combine(codeGenRunner.TemplateFolder, opts.TemplateName);

            if (!File.Exists(template))
                throw new GoliathDataException($"template file {template} not found.");

            if (string.IsNullOrWhiteSpace(opts.OutputFile))
                throw new GoliathDataException("Output file is required for generate operation. Please make sure that -out=\"YOUR_FILE.EXT\" argument is passed in.");



            var exporter = new DataExporterAdapter(providerFactory.CreateDialect(importRdbms), providerFactory.CreateDialect(exportRdbms),
                dbConnector, new TypeConverterStore());

            var counter = 0;

            List<string> errors = new List<string>();
            List<EntityMap> mapsToImport = new List<EntityMap>();

            if (!string.IsNullOrWhiteSpace(opts.Include))
            {
                var split = opts.Include.Split(new string[] { ",", "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in split)
                {

                    if (s.StartsWith("*"))
                    {
                        var q = map.EntityConfigs.Where(c => c.Name.EndsWith(s.Trim().Replace("*", string.Empty))).ToList();
                        if (q.Count > 0)
                            mapsToImport.AddRange(q);
                    }

                    if (s.EndsWith("*"))
                    {
                        var q = map.EntityConfigs.Where(c => c.Name.StartsWith(s.Trim().Replace("*", string.Empty))).ToList();
                        if (q.Count > 0)
                            mapsToImport.AddRange(q);
                    }

                    var query = map.EntityConfigs.Where(c => c.Name.Equals(s.Trim()));
                    mapsToImport.AddRange(query);
                }
            }
            else 
            {
                string[] split = new string[] { };
                if (!string.IsNullOrWhiteSpace(opts.Excluded))
                    split = opts.Excluded.Split(new string[] { ",", "|" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var mapEntityConfig in map.EntityConfigs)
                {
                    if (IsExcluded(mapEntityConfig.Name, split))
                        continue;
                    mapsToImport.Add(mapEntityConfig);
                }
            }

            foreach (var entityMap in mapsToImport)
            {
                try
                {
                    var data = exporter.Export(entityMap, opts.ExportIdentityColumn, opts.ExportDatabaseGeneratedColumns);
                    var fileName = GetFileName(entityMap.Name, entityMap.SortOrder, opts.OutputFile);

                    if (data == null || data.DataBag.Count == 0)
                    {
                        continue;
                    }

                    codeGenRunner.GenerateCodeFromTemplate(data, template, codeGenRunner.WorkingFolder, fileName);

                }
                catch (Exception ex)
                {
                    errors.Add($"Error table [{entityMap.TableName}]: {ex.ToString()}");
                }


            }

            if (errors.Count > 0)
            {
                Console.WriteLine($"\n\nEncountered {errors.Count} Errors");
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var err in errors)
                {
                    Console.WriteLine(err);
                }

                Console.ResetColor();

            }
        }

        bool IsExcluded(string entityName, string[] split)
        {
            foreach (var s in split)
            {
                if (s.StartsWith("*") && entityName.EndsWith(s.Trim().Replace("*", string.Empty)))
                {
                    return true;
                }

                else if (s.EndsWith("*") && entityName.StartsWith(s.Trim().Replace("*", string.Empty)))
                {
                    return true;
                }
                else if (entityName.Equals(s.Trim()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
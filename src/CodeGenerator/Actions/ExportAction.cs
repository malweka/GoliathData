using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;

namespace Goliath.Data.CodeGenerator.Actions
{
    class ExportAction : ActionRunner
    {

        public const string Name = "EXPORT";
        public ExportAction() : base(Name)
        {
        }

        public override void Execute(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            var codeMapFile = GetCodeMapFile(opts);
            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;
            map.Settings.AdditionalNamespaces = codeGenRunner.Settings.AdditionalNamespaces;

            var providerFactory = new ProviderFactory();

            var importRdbms = GetRdbms(opts.ImportSqlDialect);
            var exportRdbms = GetRdbms(opts.ExportSqlDialect);

            var dbConnector = providerFactory.CreateDbConnector(importRdbms, opts.ConnectionString);
            bool exportToxml = string.IsNullOrWhiteSpace(opts.TemplateName);

            if (!exportToxml && string.IsNullOrWhiteSpace(opts.TemplateName))
                throw new GoliathDataException("Template file to use is required for generate operation. Please make sure that -in=\"Template_File_name.razt\" argument is passed in.");

            var template = Path.Combine(codeGenRunner.TemplateFolder, opts.TemplateName ?? "");

            if (!exportToxml && !File.Exists(template))
                throw new GoliathDataException($"template file {template} not found.");

            if (string.IsNullOrWhiteSpace(opts.OutputFile))
                throw new GoliathDataException("Output file is required for generate operation. Please make sure that -out=\"YOUR_FILE.EXT\" argument is passed in.");

            var exporter = new DataExporterAdapter(providerFactory.CreateDialect(importRdbms), providerFactory.CreateDialect(exportRdbms),
                dbConnector, new TypeConverterStore());

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

            var files = new List<string>();

            foreach (var entityMap in mapsToImport)
            {
                try
                {
                    var data = exporter.Export(entityMap, opts.ExportIdentityColumn, opts.ExportDatabaseGeneratedColumns);
                    if (data == null || data.DataBag.Count == 0)
                    {
                        continue;
                    }

                    if (exportToxml)
                        ExportToXml(data, opts, entityMap, codeGenRunner, files);
                    else
                        ExportToSql(data, opts, entityMap, codeGenRunner, template, files);

                }
                catch (Exception ex)
                {
                    errors.Add($"Error table [{entityMap.TableName}]: {ex}");
                }
            }

            if (errors.Count > 0)
            {
                Logger.Log(LogLevel.Warning, $"\n\nEncountered {errors.Count} Errors");
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var err in errors)
                {
                    Logger.Log(LogLevel.Error, err);
                }

                throw new Exception($"Export completed with {errors.Count} errors.");
            }

            if (opts.Compress)
            {
                var dbName = FileHelperMethods.GetDatabaseNameFromConnectionString(opts.ConnectionString);
                Compress(opts,$"{dbName}_exports.zip", files);
                DeleteAll(files);
            }
        }

        void Compress(AppOptionInfo opts, string zipFileName, List<string> files)
        {
            var zipPath = Path.Combine(opts.WorkingFolder, zipFileName);
            using (var fs = File.Open(zipPath, FileMode.Create, FileAccess.Write))
            {
                fs.Zip(files.ToArray());
            }
        }

        void DeleteAll(List<string> files)
        {
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        void ExportToXml(ExportModel data, AppOptionInfo opts, EntityMap entityMap, CodeGenRunner codeGenRunner, List<string> files)
        {
            var fileName = GetFileName(entityMap.Name, null, opts.OutputFile);
            var exportedData = new ExportedDataModel
            {
                EntityName = entityMap.FullName,
                Name = entityMap.TableName,
                DataRows = data.DataBag
            };

            var filePath = Path.Combine(codeGenRunner.WorkingFolder, fileName);
            exportedData.Save(filePath);
            files.Add(filePath);
        }

        void ExportToSql(ExportModel data, AppOptionInfo opts, EntityMap entityMap, CodeGenRunner codeGenRunner, string template, List<string> files)
        {
            var fileName = GetFileName(entityMap.Name, entityMap.SortOrder, opts.OutputFile);
            codeGenRunner.GenerateCodeFromTemplate(data, template, codeGenRunner.WorkingFolder, fileName);
            files.Add(Path.Combine(codeGenRunner.WorkingFolder, fileName));
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
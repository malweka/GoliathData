using System;
using System.Collections.Generic;
using System.IO;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;

namespace Goliath.Data.CodeGenerator.Actions
{
    class GenerateEnum : ActionRunner
    {
        public const string Name = "GENERATEENUM";
        public GenerateEnum() : base(Name)
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

            if (string.IsNullOrWhiteSpace(opts.Include))
            {
                logger.Log(LogLevel.Warning, "No table to read for enum provided. Please make sure you set the include parameter properly");
                return;
            }

            var tables = opts.Include.Split(new string[] {",", ";", "|"}, StringSplitOptions.RemoveEmptyEntries);


            var exporter = new DataExporterAdapter(providerFactory.CreateDialect(importRdbms), providerFactory.CreateDialect(exportRdbms),
                dbConnector, new TypeConverterStore());

            var counter = 0;

            List<string> errors = new List<string>();
            List<ExportModel> models = new List<ExportModel>();
            foreach (var tbl in tables)
            {
                try
                {
                    logger.Log(LogLevel.Info, $"Processing entity {tbl}");
                    var entityMap = map.GetEntityMap($"{opts.Namespace}.{tbl}");

                    counter++;
                    var data = exporter.Export(entityMap, opts.ExportIdentityColumn, opts.ExportDatabaseGeneratedColumns);
                    //var fileName = GetFileName(entityMap.Name, counter, opts.OutputFile);

                    if (data == null || data.DataBag.Count == 0)
                    {
                        logger.Log(LogLevel.Warning, $"No data found for {tbl}");
                        continue;
                    }

                    models.Add(data);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error entity [{tbl}]: {ex.ToString()}");
                }
            }

            codeGenRunner.GenerateCodeFromTemplate(models, template, codeGenRunner.WorkingFolder, opts.OutputFile);
            logger.Log(LogLevel.Info, $"Enums generated file: {opts.OutputFile}");

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
    }
}
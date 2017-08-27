using System;
using System.Collections.Generic;
using System.IO;
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
            foreach (var entityMap in map.EntityConfigs)
            {
                try
                {
                    counter++;
                    var data = exporter.Export(entityMap, opts.ExportIdentityColumn, opts.ExportDatabaseGeneratedColumns);
                    var fileName = GetFileName(entityMap.Name, counter, opts.OutputFile);

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
    }
}
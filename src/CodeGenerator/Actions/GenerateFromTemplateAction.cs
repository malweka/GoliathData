using System.IO;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;

namespace Goliath.Data.CodeGenerator.Actions
{
    class GenerateFromTemplateAction : ActionRunner
    {
        public const string Name = "GENERATE";
        public GenerateFromTemplateAction() : base(Name)
        {
        }

        public override void Execute(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            var codeMapFile = GetCodeMapFile(opts);
            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;
            map.Settings.AdditionalNamespaces = codeGenRunner.Settings.AdditionalNamespaces;

            if (string.IsNullOrWhiteSpace(opts.TemplateName))
                throw new GoliathDataException("Template file to use is required for generate operation. Please make sure that -in=\"Template_File_name.razt\" argument is passed in.");

            var template = Path.Combine(codeGenRunner.TemplateFolder, opts.TemplateName);

            if (!File.Exists(template))
                throw new GoliathDataException($"template file {template} not found.");

            if (string.IsNullOrWhiteSpace(opts.OutputFile))
                throw new GoliathDataException("Output file is required for generate operation. Please make sure that -out=\"YOUR_FILE.EXT\" argument is passed in.");

            if (!Directory.Exists(codeGenRunner.WorkingFolder))
                Directory.CreateDirectory(codeGenRunner.WorkingFolder);

            if (!string.IsNullOrWhiteSpace(opts.EntityModel))
            {
                Logger.Log(LogLevel.Debug, $"Extracting model {opts.EntityModel} from map entity models.");

                if (map.EntityConfigs.TryGetValue(opts.EntityModel, out var entMap))
                {
                    codeGenRunner.GenerateCodeFromTemplate(entMap, template, codeGenRunner.WorkingFolder, opts.OutputFile);
                }
            }
            else
            {
                codeGenRunner.GenerateCodeFromTemplate(map, template, codeGenRunner.WorkingFolder, opts.OutputFile, opts.ExtendedProperties);
            }
        }
    }
}
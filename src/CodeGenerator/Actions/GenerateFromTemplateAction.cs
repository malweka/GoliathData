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

        public override void Exetute(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            var codeMapFile = GetCodeMapFile(opts);
            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;

            if (string.IsNullOrWhiteSpace(opts.TemplateName))
                throw new GoliathDataException("Template file to use is required for generate operation. Please make sure that -in=\"Template_File_name.razt\" argument is passed in.");

            var template = Path.Combine(codeGenRunner.TemplateFolder, opts.TemplateName);

            if (!File.Exists(template))
                throw new GoliathDataException(string.Format("template file {0} not found.", template));

            if (string.IsNullOrWhiteSpace(opts.OutputFile))
                throw new GoliathDataException("Output file is required for generate operation. Please make sure that -out=\"YOUR_FILE.EXT\" argument is passed in.");

            if (!string.IsNullOrWhiteSpace(opts.EntityModel))
            {
                logger.Log(LogLevel.Debug, string.Format("Extracting model {0} from map entity models.", opts.EntityModel));

                EntityMap entMap;
                if (map.EntityConfigs.TryGetValue(opts.EntityModel, out entMap))
                {
                    codeGenRunner.GenerateCodeFromTemplate(entMap, template, codeGenRunner.WorkingFolder, opts.OutputFile);
                }

                //TODO: nice to have feature is to use reflection to load a type from an external assembly.
            }
            else
            {
                codeGenRunner.GenerateCodeFromTemplate(map, template, codeGenRunner.WorkingFolder, opts.OutputFile);
            }
        }
    }
}
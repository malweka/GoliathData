using System;
using System.IO;
using Goliath.Data.Mapping;

namespace Goliath.Data.CodeGenerator.Actions
{
    class GenerateAllAction : ActionRunner
    {
        public const string Name = "GENERATEALL";
        public GenerateAllAction() : base(Name)
        {
        }

        public override void Execute(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            Console.WriteLine("\n\nGenerating for all entities...");

            var codeMapFile = GetCodeMapFile(opts);

            var template = Path.Combine(codeGenRunner.TemplateFolder, opts.TemplateName);

            if (!File.Exists(template))
                throw new GoliathDataException(string.Format("template file {0} not found.", template));

            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;

            codeGenRunner.GenerateClassesFromTemplate(map, template, codeGenRunner.WorkingFolder, (name, iteration) => GetFileName(name, iteration, opts.OutputFile), opts.ExcludedArray);

        }
    }
}
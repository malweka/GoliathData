using System;
using System.IO;
using Goliath.Data.Mapping;

namespace Goliath.Data.CodeGenerator.Actions
{
    class GenerateEntitiesAction : ActionRunner
    {
        public const string Name = "GENERATEENTITIES";
        public GenerateEntitiesAction() : base(Name)
        {
        }

        public override void Execute(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            Console.WriteLine("\n\nGenerating Entities...");

            var codeMapFile = GetCodeMapFile(opts);

            var template = Path.Combine(codeGenRunner.TemplateFolder, "TrackableClass.razt");

            if (!File.Exists(template))
                throw new GoliathDataException($"template file {template} not found.");

            if (!Directory.Exists(codeGenRunner.WorkingFolder))
                Directory.CreateDirectory(codeGenRunner.WorkingFolder);

            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;
            map.Settings.AdditionalNamespaces = codeGenRunner.Settings.AdditionalNamespaces;

            codeGenRunner.GenerateClasses(map, opts.ExtendedProperties, opts.ExcludedArray);
        }
    }
}
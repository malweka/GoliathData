using System.IO;
using Goliath.Data.Mapping;

namespace Goliath.Data.CodeGenerator.Actions
{
    class CombineMapsAction : ActionRunner
    {
        public const string Name = "COMBINEMAPS";
        public CombineMapsAction() : base(Name)
        {
        }

        public override void Execute(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            var codeMapFile = GetCodeMapFile(opts);
            var mainMap = MapConfig.Create(codeMapFile, true);

            string mapfile2 = opts.TemplateName;
            if (!File.Exists(mapfile2))
            {
                throw new GoliathDataException(string.Format("map file {0} does not exist", mapfile2));
            }

            var secondMap = MapConfig.Create(mapfile2, true);
            mainMap.MergeMap(secondMap);
            CodeGenRunner.ProcessMappedStatements(mainMap);
            mainMap.Save(codeMapFile, true);
        }
    }
}
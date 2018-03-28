using System;
using System.Collections.Generic;

namespace Goliath.Data.CodeGenerator.Actions
{
    class ActionFactory
    {
        static readonly Dictionary<string, IActionRunner> runners = new Dictionary<string, IActionRunner>();

        static ActionFactory()
        {
            runners.Add(CombineMapsAction.Name, new CombineMapsAction());
            runners.Add(CreateMapAction.Name, new CreateMapAction());
            runners.Add(ExportAction.Name, new ExportAction());
            runners.Add(ImportAction.Name, new ImportAction());
            runners.Add(GenerateAllAction.Name, new GenerateAllAction());
            runners.Add(GenerateEntitiesAction.Name, new GenerateEntitiesAction());
            runners.Add(GenerateEnum.Name, new GenerateEnum());
            runners.Add(GenerateFromTemplateAction.Name, new GenerateFromTemplateAction());
        }

        public static IActionRunner GetRunner(string actionName)
        {
            IActionRunner runner;

            if (!runners.TryGetValue(actionName, out runner))
                throw new Exception($"No action runner for action [{actionName}] found.");

            return runner;
        }
    }
}
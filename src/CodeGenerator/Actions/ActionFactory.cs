using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.CodeGenerator.Actions
{
    class ActionFactory
    {
        readonly Dictionary<string, IActionRunner> runners = new Dictionary<string, IActionRunner>();
        static readonly ILogger logger = Logger.GetLogger(typeof(Program));

        public ActionFactory(string pluginFolder = null)
        {
            runners.Add(CombineMapsAction.Name, new CombineMapsAction());
            runners.Add(CreateMapAction.Name, new CreateMapAction());
            runners.Add(ExportAction.Name, new ExportAction());
            runners.Add(ImportAction.Name, new ImportAction());
            runners.Add(GenerateAllAction.Name, new GenerateAllAction());
            runners.Add(GenerateEntitiesAction.Name, new GenerateEntitiesAction());
            runners.Add(GenerateEnum.Name, new GenerateEnum());
            runners.Add(GenerateFromTemplateAction.Name, new GenerateFromTemplateAction());
            LoadPlugins(pluginFolder);
        }

        public IActionRunner GetRunner(string actionName)
        {
            IActionRunner runner;

            if (!runners.TryGetValue(actionName, out runner))
                throw new Exception($"No action runner for action [{actionName}] found.");

            return runner;
        }

        void LoadPlugins(string pluginFolder)
        {
            if (string.IsNullOrWhiteSpace(pluginFolder) || !Directory.Exists(pluginFolder))
            {
                return;
            }

            var actionInterfaceType = typeof(IActionRunner);

            var plugins = Directory.GetFiles(pluginFolder, "Goliath.Data.*.dll");

            foreach (var plugin in plugins)
            {
                logger.Log(LogLevel.Debug, $"Loading plugin: {plugin}.");
                if("Goliath.Data.CodeGenerator.dll".Equals(Path.GetFileName(plugin)))
                    continue;

                try
                {
                    var assembly = Assembly.LoadFile(plugin);
                    var foundActions = assembly.GetTypes().Where(t => actionInterfaceType.IsAssignableFrom(t)).ToList();
                    logger.Log(LogLevel.Debug, $"Found {foundActions.Count} actions.");
                    foreach (var action in foundActions)
                    {
                        var actionRunner = (IActionRunner)Activator.CreateInstance(action);
                        runners.Add(actionRunner.ActionName, actionRunner);
                        logger.Log(LogLevel.Debug, $"Loaded {actionRunner.ActionName} command.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogException("Failed to load plugin.", ex);
                }
            }
        }
    }
}
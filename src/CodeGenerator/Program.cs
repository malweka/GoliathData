﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Goliath.Data.CodeGenerator.Actions;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;


namespace Goliath.Data.CodeGenerator
{
    class Program
    {
        static readonly ILogger logger;

        static Program()
        {
            logger = Logger.GetLogger(typeof(Program));
        }


        static void Main(string[] args)
        {

            var opts = AppOptionHandler.ParseOptions(args);

            Console.WriteLine("Starting application. Generated files will be saved on Folder: {0} ", opts.WorkingFolder);
            Console.WriteLine("Template Folder: {0} \n", opts.TemplateFolder);

            //can we load sqlite
            Console.WriteLine("Loading sqlite provider");
            var sqlite = new Goliath.Data.Sqlite.SqliteDialect();
            Console.WriteLine("Loading postgresql provider");
            var postgres = new Goliath.Data.Postgres.PostgresDialect();

            var rng = new Goliath.Security.RandomStringGenerator();

            SupportedRdbms rdbms;

            if (!string.IsNullOrWhiteSpace(opts.ProviderName))
            {
                switch (opts.ProviderName.ToUpper())
                {
                    case "MSSQL2008":
                        rdbms = SupportedRdbms.Mssql2008;
                        break;
                    case "MSSQL2008R2":
                        rdbms = SupportedRdbms.Mssql2008R2;
                        break;
                    case "POSTGRESQL8":
                        rdbms = SupportedRdbms.Postgresql8;
                        break;
                    case "POSTGRESQL9":
                        rdbms = SupportedRdbms.Postgresql9;
                        break;
                    case "SQLITE3":
                        rdbms = SupportedRdbms.Sqlite3;
                        break;
                    default:
                        rdbms = SupportedRdbms.Mssql2008;
                        break;
                }
            }

            else rdbms = SupportedRdbms.Mssql2008R2;

            if (string.IsNullOrWhiteSpace(opts.QueryProviderName))
                opts.QueryProviderName = SupportedRdbms.Mssql2008R2.ToString();

            Console.Write("Provider");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" {0} ", rdbms);
            Console.ResetColor();
            Console.Write("Activated.");
            Console.WriteLine("\n\nLoading settings...");

            var codeGenRunner = new CodeGenRunner(rdbms, new GenericCodeGenerator())
            {
                TemplateFolder = opts.TemplateFolder,
                ScriptFolder = AppDomain.CurrentDomain.BaseDirectory,
                DatabaseFolder = AppDomain.CurrentDomain.BaseDirectory,
                WorkingFolder = opts.WorkingFolder,
                QueryProviderName = opts.QueryProviderName,
                Settings = {
                    Namespace = opts.Namespace,
                    AssemblyName = opts.AssemblyName,
                    ConnectionString = opts.ConnectionString,
                    SupportManyToMany = opts.SupportManyToMany,
                    SupportTableInheritance = opts.SupportTableInheritance,
                    Platform = rdbms.ToString()
                }
            };

            if (!string.IsNullOrWhiteSpace(opts.AdditionalNameSpaces))
            {
                var split = opts.AdditionalNameSpaces.Split(new string[] {",", "|"},
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var s in split)
                {
                    codeGenRunner.Settings.AdditionalNamespaces.Add(s);
                }
            }

            var action = opts.ActionName.ToUpper();
            var actionFactory = new ActionFactory(opts.PluginFolder);
            var actionRunner = actionFactory.GetRunner(action);

            try
            {

                actionRunner.Execute(opts, codeGenRunner);
                logger.Log(LogLevel.Debug, $"Action {actionRunner.ActionName} complete.{Environment.NewLine}-- Working Folder:{opts.WorkingFolder}"

                    + $"{Environment.NewLine}-- Map file:{opts.MapFile}"

                    );
            }
            catch (Exception ex)
            {
                PrintError("Exception thrown while trying to generate all.", ex);
                Environment.Exit(2);
            }

            Console.WriteLine("\nDone!");
            Environment.Exit(0);
        }

        static void PrintError(string errorMessage, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: {0}", errorMessage);
            Console.WriteLine(ex);
            Console.ResetColor();
            Console.WriteLine("\n\nExited with errors...");
        }
    }
}

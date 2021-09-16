using System;
using System.Collections.Generic;
using System.IO;
using Goliath.Data.Generators;
using RazorEngine;
using Goliath.Data.Utils;
using Encoding = System.Text.Encoding;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Goliath.Data.CodeGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public class RazorInterpreter : IInterpreter
    {
        #region ICodeGenerator Members

        /// <summary>
        /// Generates the specified template.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="model">The model.</param>
        /// <param name="properties"></param>
        public void Generate<TModel>(Stream template, Stream outputStream, TModel model, IDictionary<string, string> properties = null)
        {
            string templateAsString = template.ConvertToString();
            Generate(templateAsString, outputStream, model, properties);
        }


        /// <summary>
        /// Generates the specified template text.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="templateText">The template text.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="model">The model.</param>
        /// <param name="properties"></param>
        public void Generate<TModel>(string templateText, Stream outputStream, TModel model, IDictionary<string, string> properties = null)
        {
            var result = CompileTemplate(templateText, model, properties);
            byte[] fileArray = Encoding.UTF8.GetBytes(result);
            outputStream.Write(fileArray, 0, fileArray.Length);
        }

        public string CompileTemplate<TModel>(string templateText, TModel model, IDictionary<string, string> properties = null)
        {
            var config = new TemplateServiceConfiguration
            {
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { })
            };

            DynamicViewBag bag = new DynamicViewBag();
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    bag.AddValue(property.Key, property.Value);
                }
            }
         
            var service = RazorEngineService.Create(config);
            var result = service.RunCompile(templateText, "key", typeof(TModel), model, bag);
            return result;
        }


        /// <summary>
        /// Generates the specified template.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="outputFile">The output file.</param>
        /// <param name="model">The model.</param>
        /// <param name="properties"></param>
        public void Generate<TModel>(string template, string outputFile, TModel model, IDictionary<string, string> properties = null)
        {
            using (var stream = File.OpenRead(template))
            {
                using (var output = File.Create(outputFile))
                {
                    Generate(stream, output, model, properties);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Supported RDBMS
    /// </summary>
    [Flags]
    public enum SupportedRdbms
    {
        None = 0,
        /// <summary>
        /// Microsoft SQL Server 2005
        /// </summary>
        Mssql2005 = 1,
        /// <summary>
        /// Microsoft SQL Server 2008
        /// </summary>
        Mssql2008 = 2,
        /// <summary>
        /// Microsoft SQL Server 2008 R2
        /// </summary>
        Mssql2008R2 = 4,
        /// <summary>
        /// All supported version of SQL Server from 2005 to 2008 R2
        /// </summary>
        MssqlAll = Mssql2005 | Mssql2008 | Mssql2008R2,
        /// <summary>
        /// Sqlite 3
        /// </summary>
        Sqlite3 = 8,

        Postgresql8 = 16,
        /// <summary>
        /// PostgreSQL 9.x
        /// </summary>
        Postgresql9 = 32,

        /// <summary>
        /// MySql 5
        /// </summary>
        MySql5 = 64,
        /// <summary>
        /// All systems
        /// </summary>
        All = MssqlAll | Sqlite3 | Postgresql9 | MySql5,
    }
}

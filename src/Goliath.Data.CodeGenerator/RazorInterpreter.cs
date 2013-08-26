using System;
using System.IO;
using Goliath.Data.Generators;
using RazorEngine;
using Goliath.Data.Utils;
using Encoding = System.Text.Encoding;

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
        /// <param name="mapfile">The mapfile.</param>
        public void Generate<TModel>(Stream template, Stream outputStream, TModel mapfile)
        {
            string templateAsString = template.ConvertToString();
            Generate(templateAsString, outputStream, mapfile);
        }


        /// <summary>
        /// Generates the specified template text.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="templateText">The template text.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="mapfile">The mapfile.</param>
        public void Generate<TModel>(string templateText, Stream outputStream, TModel mapfile)
        {
            string result = Razor.Parse(templateText, mapfile);
            byte[] fileArray = Encoding.UTF8.GetBytes(result);
            outputStream.Write(fileArray, 0, fileArray.Length);
        }


        /// <summary>
        /// Generates the specified template.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="outputFile">The output file.</param>
        /// <param name="mapfile">The mapfile.</param>
        public void Generate<TModel>(string template, string outputFile, TModel mapfile)
        {
            using (var stream = File.OpenRead(template))
            {
                using (var output = File.Create(outputFile))
                {
                    Generate(stream, output, mapfile);
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

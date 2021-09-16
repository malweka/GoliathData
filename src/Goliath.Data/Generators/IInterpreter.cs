using System.Collections.Generic;
using System.IO;

namespace Goliath.Data.Generators
{
    /// <summary>
    /// 
    /// </summary>
    public interface IInterpreter
    {
        /// <summary>
        /// Generates the specified template.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="model">The model.</param>
        /// <param name="properties"></param>
        void Generate<TModel>(Stream template, Stream outputStream, TModel model, IDictionary<string, string> properties = null);

        /// <summary>
        /// Generates the specified template text.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="templateText">The template text.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="model">The model.</param>
        /// <param name="properties"></param>
        void Generate<TModel>(string templateText, Stream outputStream, TModel model, IDictionary<string, string> properties = null);

        /// <summary>
        /// Generates the specified template.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="outputFile">The output file.</param>
        /// <param name="model">The model.</param>
        /// <param name="properties"></param>
        void Generate<TModel>(string template, string outputFile, TModel model, IDictionary<string, string> properties = null);
    }
}

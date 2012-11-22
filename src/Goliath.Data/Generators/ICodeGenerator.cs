using System.IO;

namespace Goliath.Data.Generators
{
    /// <summary>
    /// 
    /// </summary>
   public interface ICodeGenerator
   {
      /// <summary>
      /// Generates the specified template.
      /// </summary>
      /// <typeparam name="TModel">The type of the model.</typeparam>
      /// <param name="template">The template.</param>
      /// <param name="outputStream">The output stream.</param>
      /// <param name="mapfile">The mapfile.</param>
      void Generate<TModel>(Stream template, Stream outputStream, TModel mapfile);
      /// <summary>
      /// Generates the specified template text.
      /// </summary>
      /// <typeparam name="TModel">The type of the model.</typeparam>
      /// <param name="templateText">The template text.</param>
      /// <param name="outputStream">The output stream.</param>
      /// <param name="mapfile">The mapfile.</param>
      void Generate<TModel>(string templateText, Stream outputStream, TModel mapfile);
      /// <summary>
      /// Generates the specified template.
      /// </summary>
      /// <typeparam name="TModel">The type of the model.</typeparam>
      /// <param name="template">The template.</param>
      /// <param name="outputFile">The output file.</param>
      /// <param name="mapfile">The mapfile.</param>
      void Generate<TModel>(string template, string outputFile, TModel mapfile);
   }
}

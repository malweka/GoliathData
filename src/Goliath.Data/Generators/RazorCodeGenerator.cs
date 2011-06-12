using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Goliath.Data.Utils;
using RazorEngine;

namespace Goliath.Data.Generators
{
   public class RazorCodeGenerator : ICodeGenerator
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
         Generate<TModel>(templateAsString, outputStream, mapfile);
      }


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
}

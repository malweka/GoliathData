using System;
using System.IO;

namespace Goliath.Data.Utils
{
    /// <summary>
    /// 
    /// </summary>
   public static class IOHelpers
   {
      /// <summary>
      /// Converts to string.
      /// </summary>
      /// <param name="stream">The stream.</param>
      /// <returns></returns>
      public static string ConvertToString(this Stream stream)
      {
          if (stream == null) throw new ArgumentNullException("stream");

          stream.Position = 0;
         System.IO.StreamReader reader = new StreamReader(stream, true);
         string text = reader.ReadToEnd();
         
         return text;
      }

   }
}

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
         stream.Position = 0;
         System.IO.StreamReader reader = new StreamReader(stream, true);
         string text = reader.ReadToEnd();
         return text;
      }

     

      ///// <summary>
      ///// Reads the array.
      ///// </summary>
      ///// <param name="stream">The stream.</param>
      ///// <param name="data">The data.</param>
      //public static void ReadFromArray(this Stream stream, byte[] data)
      //{
      //   int offset = 0;
      //   int remaining = data.Length;
      //   while (remaining > 0)
      //   {
      //      int read = stream.Write(data, offset, remaining);
      //      if (read <= 0)
      //         throw new EndOfStreamException
      //             (String.Format("End of stream reached with {0} bytes left to read", remaining));
      //      remaining -= read;
      //      offset += read;
      //   }
      //}

      ///// <summary>
      ///// Reads the stream.
      ///// </summary>
      ///// <param name="stream">The stream.</param>
      ///// <returns></returns>
      //public static byte[] ReadStream(this Stream stream)
      //{
      //   byte[] buffer = new byte[32768];
      //   using (MemoryStream ms = new MemoryStream())
      //   {
      //      while (true)
      //      {
      //         int read = stream.Read(buffer, 0, buffer.Length);
      //         if (read <= 0)
      //            return ms.ToArray();
      //         ms.Write(buffer, 0, read);
      //      }
      //   }
      //}

   }
}

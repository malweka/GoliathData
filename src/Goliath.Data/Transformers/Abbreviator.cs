using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Transformers
{
    /// <summary>
    /// Table name abbreviatior contract
    /// </summary>
   public interface ITableNameAbbreviator
   {
      string Abbreviate(string original);
   }

   /// <summary>
   /// table name abbreviator
   /// </summary>
   public class DefaultTableNameAbbreviator : ITableNameAbbreviator
   {
     // readonly List<string> abbreviations = new List<string>();
      readonly Dictionary<string, int> abbreviations = new Dictionary<string, int>();

      public string Abbreviate(string original)
      {
         if (!string.IsNullOrWhiteSpace(original) && (original.Length > 4))
         {
            string abbr = original.Substring(0, 4).ToLower();
            int count;
            if (abbreviations.TryGetValue(abbr, out count))
            {
               string dupAbbr = string.Format("{0}{1}", abbr.Substring(0, 3), count);
               abbreviations[abbr] = count + 1;
               return dupAbbr;
            }
            else
            {
               abbreviations.Add(abbr, 1);
               return abbr;
            }

            
         }

         return original.ToLower();
      }
   }
}

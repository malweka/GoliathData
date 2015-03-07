using System;
using System.Collections.Generic;
using System.Linq;
using Goliath.Data.Providers;

namespace Goliath.Data.Transformers
{
    /// <summary>
    /// Table name abbreviatior contract
    /// </summary>
    public interface ITableNameAbbreviator
    {
        /// <summary>
        /// Abbreviates the specified original.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <returns></returns>
        string Abbreviate(string original);
    }

    /// <summary>
    /// table name abbreviator
    /// </summary>
    public class DefaultTableNameAbbreviator : ITableNameAbbreviator
    {
        // readonly List<string> abbreviations = new List<string>();
        readonly Dictionary<string, int> abbreviations = new Dictionary<string, int>();
        private string[] reservedWords;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTableNameAbbreviator"/> class.
        /// </summary>
        /// <param name="reservedWords">The reserved words.</param>
        public DefaultTableNameAbbreviator(params string[] reservedWords)
        {
            this.reservedWords = reservedWords ?? new string[] { };
        }

        bool IsReservedWord(string word)
        {
            return reservedWords.Contains(word.ToUpper());
        }

        string AbbreviateIfLong(string original)
        {
            string abbr;

            if (original.Length > 3)
            {
                abbr = original.Substring(0, 3).ToLower();
            }
            else
            {
                abbr = original.ToLower();
            }

            if (IsReservedWord(abbr))
                abbr = abbr + "g_x";

            return abbr;
        }

        /// <summary>
        /// Abbreviates the specified original.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <returns></returns>
        public string Abbreviate(string original)
        {
            if (string.IsNullOrWhiteSpace(original))
                throw new ArgumentNullException("Cannot abbreviate empty or null string");

            var abbr = AbbreviateIfLong(original);

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
    }
}

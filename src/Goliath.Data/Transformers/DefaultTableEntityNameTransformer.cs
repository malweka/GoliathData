using System;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.Transformers
{
    class DefaultTableEntityNameTransformer : INameTransformer<EntityMap>
    {
        //readonly List<string> prefixes = new List<string>();
        string tablePrefix;
        public DefaultTableEntityNameTransformer(string tablePrefix)
        {
            if (string.IsNullOrWhiteSpace(tablePrefix))
                tablePrefix = string.Empty;

            this.tablePrefix = tablePrefix;
            //if (tablePrefixes != null)
            //{
            //   foreach (string s in tablePrefixes)
            //      prefixes.Add(s);
            //}
        }

        #region ITransformer Members

        public string Transform(EntityMap mapModel, string original)
        {

            if (string.IsNullOrWhiteSpace(original))
                throw new ArgumentNullException("original");
            //if (string.IsNullOrWhiteSpace(tablePrefix))
            //    return original;

            try
            {
                if (original.StartsWith(tablePrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    int startIndex = original.IndexOf(tablePrefix, StringComparison.InvariantCultureIgnoreCase) + tablePrefix.Length;
                    if (startIndex < original.Length)
                    {
                        original = original.Substring(startIndex);
                    }
                }

                string val = original.Singularize() ?? original;
                val = val.Pascalize();
                return val;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return original;
            }
        }

        #endregion
    }
}

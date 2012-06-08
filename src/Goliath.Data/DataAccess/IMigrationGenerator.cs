using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace Goliath.Data.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMigrationGenerator
    {
        /// <summary>
        /// Generates the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        void Generate(string filename);
        /// <summary>
        /// Generates the specified text writer.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="namespace">The @namespace.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="version">The version.</param>
        void Generate(TextWriter textWriter, string @namespace, string className, long version);
    }  
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace Goliath.Data.Migrations
{
    public interface IMigrationGenerator
    {
        void Generate(string filename);
        void Generate(TextWriter textWriter, string @namespace, string className, long version);
    }  
}

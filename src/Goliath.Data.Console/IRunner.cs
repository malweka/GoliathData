using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.CodeGen
{
    using Generators;
    using Mapping;
    using Providers;
    using Providers.Sqlite;
    using Providers.SqlServer;
    using Sql;
    using Transformers;

    public interface IRunner
    {
        void RunQueryTests(string workingFolder);
    }

}

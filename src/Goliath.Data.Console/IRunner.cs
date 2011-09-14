using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Console
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

    public interface IGenerator
    {
        void GenerateCode(string templateFolder, string workingFolder);
        void GenerateMapping(string workingFolder, ProjectSettings settings, ComplexType baseModel, SupportedRdbms rdbms);
    }

    public enum SupportedRdbms
    {
        SqlServer2005,
        SqlServer2008,
        Sqlite3,
        Postgresql9,
    }
}

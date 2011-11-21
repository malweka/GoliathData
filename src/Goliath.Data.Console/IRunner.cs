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

    public interface IGenerator
    {
        void GenerateCode(string templateFolder, string workingFolder);
        MapConfig GenerateMapping(string workingFolder, ProjectSettings settings, ComplexType baseModel, SupportedRdbms rdbms);
    }

    public enum SupportedRdbms
    {
        Mssql2005,
        Mssql2008,
        Sqlite3,
        Postgresql9,
    }
}

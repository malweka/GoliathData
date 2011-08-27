using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Config
{
    public interface ISessionSettings
    {
        Mapping.MapConfig Map { get; }
        Providers.SqlMapper SqlMapper { get; }
        IDbConnector Connector { get; }
        IDbAccess DbAccess { get; }

        DbAccess CreateAccessor();
    }
}

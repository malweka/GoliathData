using System.Collections.Generic;
using Goliath.Data.Mapping;

namespace Goliath.Data
{
    public interface IDatabaseProvider
    {
        ISessionFactory SessionFactory { get; }
        IList<IKeyGenerator> KeyGenerators { get; }
        void Init();
    }
}
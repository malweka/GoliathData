using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.DynamicProxy
{
    public interface IProxyHydrator
    {
        void Hydrate(object instance, Type type);
    }
}

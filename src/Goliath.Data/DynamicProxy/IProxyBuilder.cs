using System;
using Goliath.Data.Mapping;

namespace Goliath.Data.DynamicProxy
{
    public interface IProxyBuilder
    {
        Type CreateProxyType(Type typeToProxy, EntityMap entityMap);
    }
}
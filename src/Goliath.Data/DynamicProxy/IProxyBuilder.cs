using System;
using Goliath.Data.Mapping;

namespace Goliath.Data.DynamicProxy
{
    public interface IProxyBuilder
    {
        Type CreateProxy(Type typeToProxy, EntityMap entityMap);
    }
}
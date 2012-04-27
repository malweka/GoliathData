﻿using System;
using System.Collections.Concurrent;

namespace Goliath.Data.DynamicProxy
{
    [Serializable]
    class ProxyCache
    {
        static readonly ConcurrentDictionary<Type, Type> proxies;

        static ProxyCache()
        {
            proxies = new ConcurrentDictionary<Type, Type>();
        }

        public void Add(Type typeToProxy, Type proxy)
        {
            proxies.TryAdd(typeToProxy, proxy);
        }

        public bool TryGetProxyType(Type typeToProxy, out Type proxy)
        {
            return proxies.TryGetValue(typeToProxy, out proxy);
        }
    }
}

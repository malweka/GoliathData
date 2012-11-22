using System;
using System.Collections.Concurrent;

namespace Goliath.Data.DynamicProxy
{

    [Serializable]
    class ProxyCache : IProxyCache
    {
        static readonly ConcurrentDictionary<string, Type> proxies;

        static ProxyCache()
        {
            proxies = new ConcurrentDictionary<string, Type>();
        }

        private readonly string cacheName;

        private ProxyCache(string name)
        {
            cacheName = name;
        }

        public void Add(Type typeToProxy, Type proxy)
        {
            proxies.TryAdd(BuildItemName(typeToProxy, cacheName), proxy);
        }

        public bool TryGetProxyType(Type typeToProxy, out Type proxy)
        {
            return proxies.TryGetValue(BuildItemName(typeToProxy, cacheName), out proxy);
        }

        static string BuildItemName(Type typeToProxy, string cacheName)
        {
            return cacheName + "#" + typeToProxy.FullName;
        }

        public static IProxyCache GetProxyCache<T>() where T : IProxyBuilder
        {
            return GetProxyCache(typeof (T));
        }

        public static IProxyCache GetProxyCache(Type proxyBuilderType)
        {
            return new ProxyCache(proxyBuilderType.FullName);
        }
    }

    public interface IProxyCache
    {
        void Add(Type typeToProxy, Type proxy);
        bool TryGetProxyType(Type typeToProxy, out Type proxy);
    }

}

using System;

namespace Goliath.Data.DynamicProxy
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProxyHydrator : IDisposable
    {
        /// <summary>
        /// Hydrates the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="type">The type.</param>
        void Hydrate(object instance, Type type);
    }
}

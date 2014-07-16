using System;

namespace Goliath.Data.DynamicProxy
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILazyObject
    {
        /// <summary>
        /// Gets the proxy of.
        /// </summary>
        /// <value>The proxy of.</value>
        Type ProxyOf { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is proxy loaded.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is proxy loaded; otherwise, <c>false</c>.
        /// </value>
        bool IsProxyLoaded { get; }
    }
}

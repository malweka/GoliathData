﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public interface IKeyGenerator
    {
        Type KeyType { get; }
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
        /// <summary>
        /// Generates the key.
        /// </summary>
        /// <returns></returns>
        Object GenerateKey();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IKeyGenerator<T> : IKeyGenerator
    {
        /// <summary>
        /// Generates this instance.
        /// </summary>
        /// <returns></returns>
        T Generate();
    }

    public interface IKeyGeneratorStore
    {
        void Add(IKeyGenerator generator);
        IKeyGenerator this[string keyGeneratorName] { get; }
    }
}

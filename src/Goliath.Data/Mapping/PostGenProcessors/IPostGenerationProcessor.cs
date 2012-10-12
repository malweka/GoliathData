﻿using System.Collections.Generic;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// interface for processor run after code generator has been processed.
    /// </summary>
    public interface IPostGenerationProcessor
    {
        /// <summary>
        /// Processes the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void Process(IDictionary<string, EntityMap> entities);
    }
}

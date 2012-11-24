using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Mapping;

namespace Goliath.Data.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityAccessorStore
    {
        private static readonly Dictionary<string, EntityAccessor> store = new Dictionary<string, EntityAccessor>();
        static readonly object padLock = new object();

        /// <summary>
        /// Adds the specified entity accessor.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="entityAccessor">The entity accessor.</param>
        /// <exception cref="System.ArgumentNullException">entityAccessor</exception>
        /// <exception cref="System.ArgumentException">entityAccessor.EntityType cannot but null</exception>
        internal void Add(string key, EntityAccessor entityAccessor)
        {
            if (entityAccessor == null)
                throw new ArgumentNullException("entityAccessor");

            if (!store.ContainsKey(key))
            {
                store.Add(key, entityAccessor);
            }
        }

        /// <summary>
        /// Gets the entity accessor and cache it.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">entityType</exception>
        public EntityAccessor GetEntityAccessor(Type entityType)
        {
            return GetEntityAccessor(entityType, new DynamicEntityMap(entityType));
        }

        /// <summary>
        /// Gets the entity accessor and cache it.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entMap">The ent map.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">entityType</exception>
        public EntityAccessor GetEntityAccessor(Type entityType, IEntityMap entMap)
        {
            if (entityType == null)
                throw new ArgumentNullException("entityType");

            EntityAccessor entityAccessor = null;
            string key = entityType.FullName;

            lock (padLock)
            {
                if (store.TryGetValue(key, out entityAccessor))
                {
                    return entityAccessor;
                }

                entityAccessor = new EntityAccessor(entityType);
                if (entMap != null)
                    entityAccessor.Load(entMap);

                Add(key, entityAccessor);
            }

            return entityAccessor;
        }
    }
}

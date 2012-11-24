using System;
using System.Collections.Generic;
using System.Reflection;
using Goliath.Data.Mapping;

namespace Goliath.Data.Utils
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class EntityAccessor
    {
        readonly Dictionary<string, PropertyAccessor> properties = new Dictionary<string, PropertyAccessor>();
        readonly object padLock = new object();

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public Type EntityType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is ready.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </value>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public Dictionary<string, PropertyAccessor> Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAccessor" /> class.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <exception cref="System.ArgumentNullException">entityType</exception>
        public EntityAccessor(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException("entityType");

            EntityType = entityType;
        }

        /// <summary>
        /// Loads the specified entity map.
        /// </summary>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="propertiesInfo">The properties info.</param>
        /// <exception cref="System.ArgumentNullException">entityMap</exception>
        public void Load(IEntityMap entityMap, PropertyInfo[] propertiesInfo = null)
        {
            if (entityMap == null)
                throw new ArgumentNullException("entityMap");

            if (IsReady)
                return;

            if (propertiesInfo == null)
                propertiesInfo = EntityType.GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Instance);

            //EntityMap superEntityMap = null;
            if ((entityMap is EntityMap) && ((EntityMap)entityMap).IsSubClass)
            {
                var superEntityMap = entityMap.Parent.GetEntityMap(entityMap.Extends);
                Load(superEntityMap, propertiesInfo);
            }

            lock (padLock)
            {
                foreach (var pinfo in propertiesInfo)
                {
                    var prop = entityMap.GetProperty(pinfo.Name);

                    //if ((prop == null) && (superEntityMap != null))
                    //    prop = superEntityMap[pinfo.Name];

                    if (prop != null)
                    {
                        prop.ClrType = pinfo.PropertyType;
                        var property = new PropertyAccessor
                                           {
                                               DeclaringType = EntityType,
                                               PropertyType = pinfo.PropertyType,
                                               PropertyName = prop.Name,
                                               GetMethod = pinfo.CreateDynamicGetMethodDelegate(),
                                               SetMethod = pinfo.CreateDynamicSetMethodDelegate(),
                                           };
                        Properties.Add(property.PropertyName, property);
                    }
                }

                IsReady = true;
            }
        }
    }
}
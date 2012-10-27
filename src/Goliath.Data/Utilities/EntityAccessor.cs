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
        /// <exception cref="System.ArgumentNullException">entityMap</exception>
        public void Load(IEntityMap entityMap)
        {
            if(entityMap == null)
                throw new ArgumentNullException("entityMap");

            if (IsReady)
                return;

            var propertiesInfo = EntityType.GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);

            EntityMap superEntityMap = null;
            if ((entityMap is EntityMap) && ((EntityMap)entityMap).IsSubClass)
            {
                superEntityMap = entityMap.Parent.GetEntityMap(entityMap.Extends);
            }

            lock (padLock)
            {
                foreach (var pinfo in propertiesInfo)
                {
                    /* NOTE: Intentionally going only 1 level up the inheritance. something like :
                     *  SuperSuperClass
                     *      SuperClass
                     *          Class
                     *          
                     *  SuperSuperClass if is a mapped entity its properties will be ignored. May be implement this later on. 
                     *  For now too ugly don't want to touch.
                     */
                    var prop = entityMap.GetProperty(pinfo.Name);

                    if ((prop == null) && (superEntityMap != null))
                        prop = superEntityMap[pinfo.Name];

                    if (prop != null)
                    {
                        prop.ClrType = pinfo.PropertyType;
                        var property = new PropertyAccessor
                                           {
                                               DeclaringType = EntityType,
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
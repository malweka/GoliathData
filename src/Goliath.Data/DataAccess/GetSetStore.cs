using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Fasterflect;

namespace Goliath.Data.DataAccess
{
    using Mapping;

    [Serializable]
    class GetSetStore
    {
        static readonly ConcurrentDictionary<Type, EntityGetSetInfo> store;

        static GetSetStore()
        {
            store = new ConcurrentDictionary<Type, EntityGetSetInfo>();
        }

        public void Add(Type type, EntityGetSetInfo getSetterInfo)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (getSetterInfo == null)
                throw new ArgumentNullException("getSetterInfo");

            store.TryAdd(type, getSetterInfo);
        }

        public void Add<T>(EntityGetSetInfo getSetterInfo)
        {          
            Add(typeof(T), getSetterInfo);
        }

        public bool TryGetValue(Type key, out EntityGetSetInfo getSetterInfo)
        {
            return store.TryGetValue(key, out getSetterInfo);
        }

        /// <summary>
        /// Gets the reflection info add if missing.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="entMap">The ent map.</param>
        /// <returns></returns>
        public EntityGetSetInfo GetReflectionInfoAddIfMissing(Type key, EntityMap entMap)
        {
            EntityGetSetInfo getSetInfo;

            if (!store.TryGetValue(key, out getSetInfo))
            {
                getSetInfo = new EntityGetSetInfo(key);
                getSetInfo.Load(entMap);
                store.TryAdd(key, getSetInfo);
            }

            return getSetInfo;
        }

        public EntityGetSetInfo Add(Type key, EntityMap entityMap)
        {
            EntityGetSetInfo info = new EntityGetSetInfo(key);
            info.Load(entityMap);
            Add(key, info);
            return info;
        }

    }

    [Serializable]
    struct PropInfo
    {
        public MemberSetter Setter { get; set; }
        public MemberGetter Getter { get; set; }
        public Type PropertType { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
     class EntityGetSetInfo
    {
        readonly Dictionary<string, PropInfo> properties;
        //readonly Dictionary<string, MemberGetter> getters;
        object lockStore = new object();

        public Dictionary<string, PropInfo> Properties
        {
            get { return properties; }
        }

        //public Dictionary<string, MemberGetter> Getters
        //{
        //    get { return getters; }
        //}

        public EntityGetSetInfo(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException("entityType");

            EntityType = entityType;

            properties = new Dictionary<string, PropInfo>();

        }

        bool loaded;
        public void Load(EntityMap map)
        {
            var propertiesInfo = EntityType.GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);

            if (!loaded)
            {
                EntityMap superEntityMap = null;
                if (map.IsSubClass)
                {
                    superEntityMap = map.Parent.GetEntityMap(map.Extends);
                }

                lock (lockStore)
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
                        var prop = map[pinfo.Name];
                        if ((prop == null) && (superEntityMap != null))
                            prop = superEntityMap[pinfo.Name];

                        if (prop != null)
                        {
                            //if (pinfo.PropertyType.Implements<System.Collections.ICollection>())
                            //    continue;

                            prop.ClrType = pinfo.PropertyType;
                            MemberSetter setter = EntityType.DelegateForSetPropertyValue(prop.PropertyName);
                            MemberGetter getter = EntityType.DelegateForGetPropertyValue(prop.PropertyName);

                            PropInfo propInfo = new PropInfo { Getter = getter, Setter = setter, Name = prop.PropertyName, PropertType = pinfo.PropertyType};
                            Properties.Add(prop.PropertyName, propInfo);
                        }
                        
                    }                    

                    loaded = true;
                }
            }
        }

        public Type EntityType { get; private set; }

    }
}

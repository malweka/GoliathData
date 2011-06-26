using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using Fasterflect;
using Goliath.Data.Mapping;
using System.Reflection;

namespace Goliath.Data.DataAccess
{
    class GetSetStore
    {
        static readonly ConcurrentDictionary<Type, EntityGetSetInfo> store;

        static GetSetStore()
        {
            store = new ConcurrentDictionary<Type, EntityGetSetInfo>();
        }

        public void Add<T>(EntityGetSetInfo getSetterInfo)
        {
            if (getSetterInfo == null)
                throw new ArgumentNullException("getSetterInfo");

            store.TryAdd(typeof(T), getSetterInfo);
        }

        public bool TryGetValue(Type key, out EntityGetSetInfo getSetterInfo)
        {
            return store.TryGetValue(key, out getSetterInfo);
        }

    }

    struct PropInfo
    {
        public MemberSetter Setter { get; set; }
        public MemberGetter Getter { get; set; }
        public Type PropertType { get; set; }
        public string Name { get; set; }
    }

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
                lock (lockStore)
                {
                    foreach (var pinfo in propertiesInfo)
                    {
                        var prop = map[pinfo.Name];
                        if (prop != null)
                        {
                            if (pinfo.PropertyType.Implements<System.Collections.ICollection>())
                                continue;

                            prop.ClrType = pinfo.PropertyType;
                            MemberSetter setter = EntityType.DelegateForSetPropertyValue(prop.PropertyName);
                            MemberGetter getter = EntityType.DelegateForGetPropertyValue(prop.PropertyName);

                            PropInfo propInfo = new PropInfo { Getter = getter, Setter = setter, Name = prop.PropertyName, PropertType = pinfo.PropertyType };
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using Fasterflect;
using Goliath.Data.Mapping;

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

    class EntityGetSetInfo
    {
        readonly Dictionary<string, MemberSetter> setters;
        readonly Dictionary<string, MemberGetter> getters;
        object lockStore = new object();

        public Dictionary<string, MemberSetter> Setters
        {
            get { return setters; }
        }

        public Dictionary<string, MemberGetter> Getters
        {
            get { return getters; }
        }

        public EntityGetSetInfo(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException("entityType");

            EntityType = entityType;

            getters = new Dictionary<string, MemberGetter>();
            setters = new Dictionary<string, MemberSetter>();

        }

        bool loaded;
        public void Load(EntityMap map)
        {
            if (!loaded)
            {
                lock (lockStore)
                {

                    if (map.PrimaryKey != null)
                    {
                        foreach (var k in map.PrimaryKey.Keys)
                        {
                            var prop = k.Key;
                            MemberSetter setter = EntityType.DelegateForSetPropertyValue(prop.PropertyName);
                            MemberGetter getter = EntityType.DelegateForGetPropertyValue(prop.PropertyName);

                            getters.Add(prop.PropertyName, getter);
                            setters.Add(prop.PropertyName, setter);
                        }
                    }

                    foreach (var prop in map.Properties)
                    {
                        MemberSetter setter = EntityType.DelegateForSetPropertyValue(prop.PropertyName);
                        MemberGetter getter = EntityType.DelegateForGetPropertyValue(prop.PropertyName);

                        getters.Add(prop.PropertyName, getter);
                        setters.Add(prop.PropertyName, setter);
                    }

                    foreach (var prop in map.Relations)
                    {
                        if (prop.RelationType != RelationshipType.OneToMany)
                            continue;

                        MemberSetter setter = EntityType.DelegateForSetPropertyValue(prop.PropertyName);
                        MemberGetter getter = EntityType.DelegateForGetPropertyValue(prop.PropertyName);

                        getters.Add(prop.PropertyName, getter);
                        setters.Add(prop.PropertyName, setter);
                    }

                    loaded = true;
                }
            }
        }

        public Type EntityType { get; private set; }

    }
}

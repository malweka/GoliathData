using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using Goliath.Data.Mapping;
using System.Data.Common;
using Goliath.Data.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntitySerializerFactory
    {
        /// <summary>
        /// Registers the entity serializer.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        void RegisterEntitySerializer<TEntity>(Func<DbDataReader, EntityMap, TEntity> factoryMethod);
        /// <summary>
        /// Serializes the specified data reader.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns>
        TEntity Serialize<TEntity>(DbDataReader dataReader, EntityMap entityMap);
    }

    class EntitySerializerFactory : IEntitySerializerFactory
    {

        static ConcurrentDictionary<Type, Delegate> factoryList = new ConcurrentDictionary<Type, Delegate>();
        static object lockFactoryList = new object();
        static ILogger logger;

        static EntitySerializerFactory()
        {
            logger = Logger.GetLogger(typeof(EntitySerializerFactory));
        }

        #region IEntitySerializerFactory Members

        public void RegisterEntitySerializer<TEntity>(Func<DbDataReader, EntityMap, TEntity> factoryMethod)
        {
            throw new NotImplementedException();
        }

        public TEntity Serialize<TEntity>(DbDataReader dataReader, EntityMap entityMap)
        {
            Delegate dlgMethod;
            Type type = typeof(TEntity);
            Func<DbDataReader, EntityMap, TEntity> factoryMethod = null;
            if (factoryList.TryGetValue(type, out dlgMethod))
            {
                if (dlgMethod is Func<DbDataReader, EntityMap, TEntity>)
                    factoryMethod = (Func<DbDataReader, EntityMap, TEntity>)dlgMethod;
                else
                    throw new GoliathDataException("unknown factory method");
            }
            else
            {
                factoryMethod = CreateSerializerMethod<TEntity>(entityMap);
                factoryList.TryAdd(type, factoryMethod);
            }           

            TEntity entity = factoryMethod.Invoke(dataReader, entityMap);
            return entity;
        }

        #endregion

        //Dapper .Net inspired
        Func<DbDataReader, EntityMap, TEntity> CreateSerializerMethod<TEntity>(EntityMap entityMap)
        {
            Type[] args = { typeof(DbDataReader), typeof(EntityMap)};
            Type type = typeof(TEntity);
            DynamicMethod dm = new DynamicMethod(string.Format("Srlz_{0}", Guid.NewGuid()), type, args, true);
            ILGenerator il = dm.GetILGenerator();
            //il.DeclareLocal(typeof(int));
            //il.DeclareLocal(type);

            //il.Emit(OpCodes.Stloc_0);            
            

            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(p => new _Prop
                {
                    Name = p.Name,
                    Setter = p.DeclaringType == type ? p.GetSetMethod(true) : p.DeclaringType.GetProperty(p.Name).GetSetMethod(true),
                    Type = p.PropertyType
                })
                .Where(info => info.Setter != null)
                .ToList();
            
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
            List<_PropSetter> setters = new List<_PropSetter>();
            if (entityMap.PrimaryKey != null)
            {
                setters = (from n in entityMap.Properties
                                let prop = properties.FirstOrDefault(p => string.Equals(p.Name, n.Name, StringComparison.InvariantCulture))
                                let field = prop != null ? null : (fields.FirstOrDefault(p => string.Equals(p.Name, n.Name, StringComparison.InvariantCulture)))
                                select new _PropSetter { Prop = prop, Field = field, Property = n }).ToList();
            }

            var pSetters = (from n in entityMap.Properties
                          let prop = properties.FirstOrDefault(p=>string.Equals(p.Name, n.Name,  StringComparison.InvariantCulture))
                          let field = prop != null ? null : (fields.FirstOrDefault(p => !n.IsPrimaryKey && string.Equals(p.Name, n.Name, StringComparison.InvariantCulture)))
                            select new _PropSetter { Prop = prop, Field = field, Property = n }).ToList();

            var rSetters = (from n in entityMap.Relations
                            let prop = properties.FirstOrDefault(p => string.Equals(p.Name, n.Name, StringComparison.InvariantCulture))
                           let field = prop != null ? null : (fields.FirstOrDefault(p => !n.IsPrimaryKey && string.Equals(p.Name, n.Name, StringComparison.InvariantCulture) && (n.RelationType == RelationshipType.OneToMany)))
                            select new _PropSetter { Prop = prop, Field = field, Property = n }).ToList();

            setters.AddRange(pSetters);
            setters.AddRange(rSetters);
            //il.Emit(OpCodes.Stloc_0);  
            //il.BeginExceptionBlock();
            il.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            

            ////var allDone = il.DefineLabel();

            //foreach (var item in setters)
            //{
            //    if (item.Property != null) //|| (item.Field != null))
            //    {
            //        il.Emit(OpCodes.Ldloc_0);
            //        il.Emit(OpCodes.Ldarg_0);
            //        il.Emit(OpCodes.Ldstr, item.Property.GetQueryName(entityMap));
            //        il.Emit(OpCodes.Callvirt, getItem);

            //        switch (item.Prop.Type.Name)
            //        {
            //            case "Int16":
            //                il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt16", new Type[] { typeof(object) }));
            //                break;
            //            case "Int32":
            //                il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt32", new Type[] { typeof(object) }));
            //                break;
            //            case "Int64":
            //                il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt64", new Type[] { typeof(object) }));
            //                break;
            //            case "Boolean":
            //                il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToBoolean", new Type[] { typeof(object) }));
            //                break;
            //            case "String":
            //                il.Emit(OpCodes.Callvirt, typeof(string).GetMethod("ToString", new Type[] { }));
            //                break;
            //            case "DateTime":
            //                il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToDateTime", new Type[] { typeof(object) }));
            //                break;
            //            case "Decimal":
            //                il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToDecimal", new Type[] { typeof(object) }));
            //                break;
            //            default:
            //                // Don't set the field value as it's an unsupported type
            //                continue;
            //        }

            //        il.Emit(OpCodes.Callvirt, item.Prop.Setter);              
            //    }
            //}

            //il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return (Func<DbDataReader, EntityMap, TEntity>)dm.CreateDelegate(typeof(Func<DbDataReader, EntityMap, TEntity>));
            //throw new NotImplementedException();
        }

        public static void DataReaderColumnList(IDataReader dataReader, EntityMap entMap)
        {
            List<string> ColumnNames = new List<string>();
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
            }
        }

        static readonly MethodInfo
                    enumParse = typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(string), typeof(bool) }),
                    getItem = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(p => p.GetIndexParameters().Any() && p.GetIndexParameters()[0].ParameterType == typeof(int))
                        .Select(p => p.GetGetMethod()).First();
        static MethodInfo fnGetValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });
        static MethodInfo fnIsDBNull = typeof(IDataRecord).GetMethod("IsDBNull");
        static MethodInfo fnListGetItem = typeof(List<Func<object, object>>).GetProperty("Item").GetGetMethod();
        static MethodInfo fnInvoke = typeof(Func<object, object>).GetMethod("Invoke");

        private static void EmitInt32(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }
        class _Prop
        {
            public string Name;
            public MethodInfo Setter;
            public Type Type;
        }

        struct _PropSetter
        {
            public _Prop Prop;
            public FieldInfo Field;
            public Property Property;
        }
    }
}

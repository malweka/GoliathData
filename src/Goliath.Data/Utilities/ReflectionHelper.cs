﻿using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Goliath.Data.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReflectionHelper
    {
        static Hashtable mTypeHash = new Hashtable();

        static ReflectionHelper()
        {
            mTypeHash[typeof(sbyte)] = OpCodes.Ldind_I1;
            mTypeHash[typeof(byte)] = OpCodes.Ldind_U1;
            mTypeHash[typeof(char)] = OpCodes.Ldind_U2;
            mTypeHash[typeof(short)] = OpCodes.Ldind_I2;
            mTypeHash[typeof(ushort)] = OpCodes.Ldind_U2;
            mTypeHash[typeof(int)] = OpCodes.Ldind_I4;
            mTypeHash[typeof(uint)] = OpCodes.Ldind_U4;
            mTypeHash[typeof(long)] = OpCodes.Ldind_I8;
            mTypeHash[typeof(ulong)] = OpCodes.Ldind_I8;
            mTypeHash[typeof(bool)] = OpCodes.Ldind_I1;
            mTypeHash[typeof(double)] = OpCodes.Ldind_R8;
            mTypeHash[typeof(float)] = OpCodes.Ldind_R4;
        }

        static string GetMethodName(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            string propName = "get_" + property.DeclaringType.FullName.Replace(".", "_") + property.Name;
            return propName;
        }

        public static Func<object, object> CreateDynamicGetMethodDelegate(this PropertyInfo property)
        {
            return CreateDynamicGetMethodDelegate(property, GetMethodName(property));
        }

        /// <summary>
        /// Creates dynamic get method delegate for property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">property</exception>
        public static Func<object, object> CreateDynamicGetMethodDelegate(this PropertyInfo property, string methodName)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            Debug.Assert(property.DeclaringType != null, "property.DeclaringType != null");
            var method = new DynamicMethod(methodName, typeof(object), new[] { typeof(object) });
            var gen = method.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Castclass, property.DeclaringType);
            gen.Emit(OpCodes.Callvirt, property.GetGetMethod());

            if (property.PropertyType.IsValueType)
                gen.Emit(OpCodes.Box, property.PropertyType);

            gen.Emit(OpCodes.Ret);

            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        public static Action<object, object> CreateDynamicSetMethodDelegate(this PropertyInfo property)
        {
            return CreateDynamicSetMethodDelegate(property, GetMethodName(property));
        }

        /// <summary>
        /// Creates the dynamic set method delegate.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">property</exception>
        public static Action<object, object> CreateDynamicSetMethodDelegate(this PropertyInfo property, string methodName)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            var setMethod = property.GetSetMethod();
            if (setMethod == null)
                return null;

            var arguments = new Type[] {typeof (object), typeof (object)};

            Debug.Assert(property.DeclaringType != null, "property.DeclaringType != null");
            var setter = new DynamicMethod(methodName, typeof(void), arguments, property.DeclaringType);
            var generator = setter.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, property.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);

            //if (property.PropertyType.IsClass)
            //    generator.Emit(OpCodes.Castclass, property.PropertyType);
            //else
            //    generator.Emit(OpCodes.Unbox_Any, property.PropertyType);

            if(property.PropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox, property.PropertyType);
                if (mTypeHash[property.PropertyType] != null)
                {
                    OpCode load = (OpCode)mTypeHash[property.PropertyType];
                    generator.Emit(load);
                }
                else
                {
                    generator.Emit(OpCodes.Ldobj, property.PropertyType);
                }
            }
            else
            {
                generator.Emit(OpCodes.Castclass, property.PropertyType);
            }

            generator.EmitCall(OpCodes.Callvirt, setMethod, null);
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>)setter.CreateDelegate(typeof(Action<object, object>));

        }

        //public static Action<object, object> CreateDynamicSetMethodDelegate(this PropertyInfo property, string methodName)
        //{
        //    if (property == null)
        //        throw new ArgumentNullException("property");

        

        //    var method = new DynamicMethod(methodName, null, new[] { typeof(object), typeof(object) });
        //    ILGenerator gen = method.GetILGenerator();
        //    var paramType = property.PropertyType;

        //    gen.DeclareLocal(paramType);
        //    gen.Emit(OpCodes.Ldarg_1); //load first argument
        //    gen.Emit(OpCodes.Castclass, property.DeclaringType);
        //    gen.Emit(OpCodes.Ldarg_2); //load second argument
        //    if (paramType.IsValueType)
        //    {
        //        gen.Emit(OpCodes.Unbox, paramType);
        //        if (mTypeHash[paramType] != null) //and load
        //        {
        //            OpCode load = (OpCode)mTypeHash[paramType];
        //            gen.Emit(load);
        //        }
        //        else
        //        {
        //            gen.Emit(OpCodes.Ldobj, paramType);
        //        }
        //    }
        //    else
        //    {
        //        gen.Emit(OpCodes.Castclass, paramType);
        //    }

        //    gen.EmitCall(OpCodes.Callvirt, property.GetSetMethod(), null);
        //    gen.Emit(OpCodes.Ret);

        //    return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        //}
    }
}

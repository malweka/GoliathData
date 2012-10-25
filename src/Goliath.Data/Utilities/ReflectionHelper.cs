using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;

namespace Goliath.Data.Utils
{
    public static class ReflectionHelper
    {
        public static Func<object, object> CreateDynamicGetMethodDelegate(this PropertyInfo property, string methodName)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            var method = new DynamicMethod(methodName, typeof(object), new[] { typeof(object) });
            ILGenerator gen = method.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Castclass, property.DeclaringType);
            gen.Emit(OpCodes.Callvirt, property.GetGetMethod());
            gen.Emit(OpCodes.Ret);

            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        public static Action<object, object> CreateDynamicSetMethodDelegate(this PropertyInfo property, string methodName)
        {
            MethodInfo setMethod = property.GetSetMethod();
            if (setMethod == null)
                return null;

            Type[] arguments = new Type[] {typeof (object), typeof (object)};

            DynamicMethod setter = new DynamicMethod(methodName, typeof(void), arguments, property.DeclaringType);
            ILGenerator generator = setter.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, property.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);

            if (property.PropertyType.IsClass)
                generator.Emit(OpCodes.Castclass, property.PropertyType);
            else
                generator.Emit(OpCodes.Unbox_Any, property.PropertyType);

            generator.EmitCall(OpCodes.Callvirt, setMethod, null);
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>)setter.CreateDelegate(typeof(Action<object, object>));

        }

        //public static Action<object, object> CreateDynamicSetMethodDelegate(this PropertyInfo property, string methodName)
        //{
        //    if (property == null)
        //        throw new ArgumentNullException("property");

        //    var mTypeHash = new Hashtable();
        //    mTypeHash[typeof(sbyte)] = OpCodes.Ldind_I1;
        //    mTypeHash[typeof(byte)] = OpCodes.Ldind_U1;
        //    mTypeHash[typeof(char)] = OpCodes.Ldind_U2;
        //    mTypeHash[typeof(short)] = OpCodes.Ldind_I2;
        //    mTypeHash[typeof(ushort)] = OpCodes.Ldind_U2;
        //    mTypeHash[typeof(int)] = OpCodes.Ldind_I4;
        //    mTypeHash[typeof(uint)] = OpCodes.Ldind_U4;
        //    mTypeHash[typeof(long)] = OpCodes.Ldind_I8;
        //    mTypeHash[typeof(ulong)] = OpCodes.Ldind_I8;
        //    mTypeHash[typeof(bool)] = OpCodes.Ldind_I1;
        //    mTypeHash[typeof(double)] = OpCodes.Ldind_R8;
        //    mTypeHash[typeof(float)] = OpCodes.Ldind_R4;

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

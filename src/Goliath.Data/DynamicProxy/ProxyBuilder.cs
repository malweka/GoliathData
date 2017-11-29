﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using Goliath.Data.Mapping;

namespace Goliath.Data.DynamicProxy
{
    /// <summary>
    /// 
    /// </summary>
    public class ProxyBuilder : IProxyBuilder
    {

        const string LazyObjectTriggerFieldName = "_lazyObjectLoaded";
        const string LazyObjectProxyFieldName = "_lazyObjectProxyOf";
        const string LazyObjectProxyHydrator = "_lazyObjectHydrator";

        static readonly ModuleBuilder moduleBuilder;

        static ProxyBuilder()
        {
            var proxyAssemblyName = new AssemblyName("GoliathData_Proxy" + Guid.NewGuid().ToString("N"));
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(proxyAssemblyName, AssemblyBuilderAccess.RunAndSave);
            moduleBuilder = asmBuilder.DefineDynamicModule("GoliathDataProxies");
        }

        /// <summary>
        /// Adds  interface implementations.
        /// </summary>
        /// <param name="typeBuilder">The type builder.</param>
        protected virtual void AddInterfaceImplementation(TypeBuilder typeBuilder)
        {
            typeBuilder.AddInterfaceImplementation(typeof(ILazyObject));
        }

        /// <summary>
        /// Creates the proxy.
        /// </summary>
        /// <param name="typeToProxy">The type to proxy.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <returns></returns> 
        public virtual Type CreateProxyType(Type typeToProxy, EntityMap entityMap)
        {
            Type proxyType;
            var pcache = ProxyCache.GetProxyCache(GetType());
            if (!pcache.TryGetProxyType(typeToProxy, out proxyType))
            {
                var typeBuilder = moduleBuilder.DefineType(string.Format("LazyObject_{0}{1}", typeToProxy.Name, Guid.NewGuid().ToString("N")), TypeAttributes.Public);

                var fieldBuilderIsLoaded = typeBuilder.DefineField(LazyObjectTriggerFieldName, typeof(bool), FieldAttributes.Private);
                var fieldBuilderProxyOf = typeBuilder.DefineField(LazyObjectProxyFieldName, typeof(Type), FieldAttributes.Private);
                var fieldBuilderHydrator = typeBuilder.DefineField(LazyObjectProxyHydrator, typeof(IProxyHydrator), FieldAttributes.Private);
                //var methods = typeToProxy.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                var properties = typeToProxy.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

                typeBuilder.SetParent(typeToProxy);
                AddInterfaceImplementation(typeBuilder);

                var ctorBuilder = BuildConstructor(typeBuilder, typeToProxy, fieldBuilderProxyOf, fieldBuilderIsLoaded, fieldBuilderHydrator);
                var loadMeMethodBuilder = BuildLoadMeMethod(typeBuilder, fieldBuilderProxyOf, fieldBuilderIsLoaded, fieldBuilderHydrator);

                CreateILazyObjectProperties(typeBuilder, fieldBuilderProxyOf, fieldBuilderIsLoaded);

                foreach (var pinfo in properties)
                {
                    if (entityMap.ContainsProperty(pinfo.Name))
                    {
                        OverrideGetProperty(pinfo, typeToProxy, typeBuilder, loadMeMethodBuilder);
                        OverrideSetProperty(pinfo, typeToProxy, typeBuilder, loadMeMethodBuilder);
                    }
                }

                proxyType = typeBuilder.CreateType();
                pcache.Add(typeToProxy, proxyType);
            }

            return proxyType;
        }

        /// <summary>
        /// Builds the constructor.
        /// </summary>
        /// <param name="typeBuilder">The type builder.</param>
        /// <param name="baseClass">The base class.</param>
        /// <param name="typeProxy">The type proxy.</param>
        /// <param name="isloaded">The isloaded.</param>
        /// <param name="proxyHydra">The proxy hydra.</param>
        /// <returns></returns>
        protected virtual MethodBuilder BuildConstructor(TypeBuilder typeBuilder, Type baseClass, FieldBuilder typeProxy,
            FieldBuilder isloaded, FieldBuilder proxyHydra)
        {
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;
            MethodBuilder method = typeBuilder.DefineMethod(".ctor", methodAttributes);

            ConstructorInfo ctor1 = baseClass.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { },
                null
                );

            method.SetReturnType(typeof(void));
            // Adding parameters
            method.SetParameters(
                typeof(Type),
                typeof(IProxyHydrator)
                );
            // Parameter typeToProxy
            ParameterBuilder typeToProxy = method.DefineParameter(1, ParameterAttributes.None, "typeToProxy");
            // Parameter proxyHydrator
            ParameterBuilder proxyHydrator = method.DefineParameter(2, ParameterAttributes.None, "proxyHydrator");
            ILGenerator gen = method.GetILGenerator();
            // Writing body
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, ctor1);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, typeProxy);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Stfld, isloaded);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Stfld, proxyHydra);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);

            // finished
            return method;

        }

        /// <summary>
        /// Builds the load me method.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="typeProxy">The type proxy.</param>
        /// <param name="isloaded">The isloaded.</param>
        /// <param name="proxyHydra">The proxy hydra.</param>
        /// <returns></returns>
        protected virtual MethodBuilder BuildLoadMeMethod(TypeBuilder type, FieldBuilder typeProxy,
            FieldBuilder isloaded, FieldBuilder proxyHydra)
        {
            // Declaring method builder
            // Method attributes
            MethodAttributes methodAttributes = MethodAttributes.Private
                | MethodAttributes.HideBySig;

            MethodBuilder method = type.DefineMethod("LoadMe", methodAttributes);
            MethodInfo hydrateMethod = typeof(IProxyHydrator).GetMethod(
                "Hydrate",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(Object), typeof(Type) },
                null);

            MethodInfo disposeMethod = typeof(IDisposable).GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { },
                null);

            // Setting return type
            method.SetReturnType(typeof(void));
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder loclabuilder = gen.DeclareLocal(typeof(Boolean));
            // Preparing labels
            Label label39 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, isloaded);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Brtrue_S, label39);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, proxyHydra);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, typeProxy);
            gen.Emit(OpCodes.Callvirt, hydrateMethod);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Stfld, isloaded);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, proxyHydra);
            gen.Emit(OpCodes.Callvirt, disposeMethod);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.MarkLabel(label39);
            gen.Emit(OpCodes.Ret);

            return method;

        }

        /// <summary>
        /// Creates the ILazyObject properties.
        /// </summary>
        /// <param name="typeBuilder">The type builder.</param>
        /// <param name="typeProxy">The type proxy.</param>
        /// <param name="isloaded">The isloaded.</param>
        protected virtual void CreateILazyObjectProperties(TypeBuilder typeBuilder, FieldBuilder typeProxy, FieldBuilder isloaded)
        {
            MethodAttributes methodAttributes = MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.Final
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot;

            var isLoadedPropBuilder = typeBuilder.DefineProperty("IsProxyLoaded", PropertyAttributes.None, typeof(Boolean), null);
            MethodBuilder isLoadedGetMethod = typeBuilder.DefineMethod("get_IsProxyLoaded", methodAttributes);
            isLoadedGetMethod.SetReturnType(typeof(Boolean));
            ILGenerator isLoadedGen = isLoadedGetMethod.GetILGenerator();
            LocalBuilder isLoadedLocalBuilder = isLoadedGen.DeclareLocal(typeof(Boolean));
            Label label11 = isLoadedGen.DefineLabel();
            isLoadedGen.Emit(OpCodes.Nop);
            isLoadedGen.Emit(OpCodes.Ldarg_0);
            isLoadedGen.Emit(OpCodes.Ldfld, isloaded);
            isLoadedGen.Emit(OpCodes.Stloc_0);
            isLoadedGen.Emit(OpCodes.Br_S, label11);
            isLoadedGen.MarkLabel(label11);
            isLoadedGen.Emit(OpCodes.Ldloc_0);
            isLoadedGen.Emit(OpCodes.Ret);
            isLoadedPropBuilder.SetGetMethod(isLoadedGetMethod);

            var proxyPropBuilder = typeBuilder.DefineProperty("ProxyOf", PropertyAttributes.None, typeof(Type), null);
            MethodBuilder proxyOfGetMethod = typeBuilder.DefineMethod("get_ProxyOf", methodAttributes);
            proxyOfGetMethod.SetReturnType(typeof(Type));
            ILGenerator proxyOfgen = proxyOfGetMethod.GetILGenerator();
            LocalBuilder proxyOfLocalBuilder = proxyOfgen.DeclareLocal(typeof(Type));
            Label label10 = proxyOfgen.DefineLabel();
            proxyOfgen.Emit(OpCodes.Nop);
            proxyOfgen.Emit(OpCodes.Ldarg_0);
            proxyOfgen.Emit(OpCodes.Ldfld, typeProxy);
            proxyOfgen.Emit(OpCodes.Stloc_0);
            proxyOfgen.Emit(OpCodes.Br_S, label10);
            proxyOfgen.MarkLabel(label10);
            proxyOfgen.Emit(OpCodes.Ldloc_0);
            proxyOfgen.Emit(OpCodes.Ret);
            proxyPropBuilder.SetGetMethod(proxyOfGetMethod);


        }

        /// <summary>
        /// Overrides the get property.
        /// </summary>
        /// <param name="propertyAccessor">The property accessor.</param>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="type">The type.</param>
        /// <param name="loadMeMethodBuilder">The load me method builder.</param>
        /// <returns></returns>
        protected virtual MethodBuilder OverrideGetProperty(PropertyInfo propertyAccessor, Type baseType, TypeBuilder type, MethodBuilder loadMeMethodBuilder)
        {
            System.Reflection.MethodAttributes methodAttributes = MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.HideBySig;

            string methodName = "get_" + propertyAccessor.Name;
            MethodBuilder method = type.DefineMethod(methodName, methodAttributes);
            // Preparing Reflection instances

            MethodInfo baseMethod = baseType.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { },
                null
                );
            // Setting return type
            method.SetReturnType(propertyAccessor.PropertyType);
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder localBuilder = gen.DeclareLocal(propertyAccessor.PropertyType);
            // Preparing labels
            Label label17 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, loadMeMethodBuilder);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, baseMethod);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label17);
            gen.MarkLabel(label17);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);

            return method;

        }

        /// <summary>
        /// Overrides the set property.
        /// </summary>
        /// <param name="propertyAccessor">The property accessor.</param>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="type">The type.</param>
        /// <param name="loadMeMethodBuilder">The load me method builder.</param>
        /// <returns></returns>
        protected virtual MethodBuilder OverrideSetProperty(PropertyInfo propertyAccessor, Type baseType, TypeBuilder type, MethodBuilder loadMeMethodBuilder)
        {
            System.Reflection.MethodAttributes methodAttributes = MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.HideBySig;

            string methodName = "set_" + propertyAccessor.Name;
            MethodBuilder method = type.DefineMethod(methodName, methodAttributes);
            // Preparing Reflection instances

            MethodInfo baseMethod = baseType.GetMethod(methodName);

            // Setting return type
            method.SetReturnType(typeof(void));
            // Adding parameters
            method.SetParameters(propertyAccessor.PropertyType);
            // Parameter value
            ParameterBuilder value = method.DefineParameter(1, ParameterAttributes.None, "value");
            ILGenerator gen = method.GetILGenerator();
            // Writing body
            //gen.Emit(OpCodes.Nop);
            //gen.Emit(OpCodes.Ldarg_0);
            //gen.Emit(OpCodes.Call, loadMeMethodBuilder);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, baseMethod);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);

            return method;
        }


    }
}

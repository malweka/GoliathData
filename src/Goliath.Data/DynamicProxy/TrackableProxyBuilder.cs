using System;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using Goliath.Data.Entity;
using Goliath.Data.Mapping;

namespace Goliath.Data.DynamicProxy
{
    public class TrackableProxyBuilder : ProxyBuilder
    {

        const string LazyObjectTriggerFieldName = "_lazyObjectLoaded";
        const string LazyObjectProxyFieldName = "_lazyObjectProxyOf";
        const string LazyObjectProxyHydrator = "_lazyObjectHydrator";

        static readonly ModuleBuilder moduleBuilder;

        protected override void AddInterfaceImplementation(TypeBuilder typeBuilder)
        {
            base.AddInterfaceImplementation(typeBuilder);
            typeBuilder.AddInterfaceImplementation(typeof(ITrackable));
            typeBuilder.AddInterfaceImplementation(typeof(INotifyPropertyChanged));
        }

        protected override MethodBuilder BuildConstructor(TypeBuilder typeBuilder, Type baseClass, FieldBuilder typeProxy,
            FieldBuilder isloaded, FieldBuilder proxyHydra)
        {
            FieldBuilder changeTrackField = typeBuilder.DefineField("_changeTracker", typeof(IChangeTracker), FieldAttributes.Private);
            FieldBuilder propertyChangedField = typeBuilder.DefineField("PropertyChanged", typeof(PropertyChangedEventHandler), FieldAttributes.Private);

            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;
            MethodBuilder method = typeBuilder.DefineMethod(".ctor", methodAttributes);

            ConstructorInfo ctor1 = baseClass.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { },
                null
                );

            ConstructorInfo ctor5 = typeof(ChangeTracker).GetConstructor(
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
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Newobj, ctor5);
            gen.Emit(OpCodes.Stfld, changeTrackField);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);

            
            var getChangeTrackerMethod = CreateChangeTrackerProperty(typeBuilder, changeTrackField);
            BuildNotifyMethod(typeBuilder, getChangeTrackerMethod, propertyChangedField);

            // finished
            return method;
        }

        //protected override MethodBuilder BuildLoadMeMethod(TypeBuilder type, Type baseClass, FieldBuilder typeProxy,
        //    FieldBuilder isloaded, FieldBuilder proxyHydra)
        //{
        //    BuildNotifyMethod(type, baseClass);
        //    return base.BuildLoadMeMethod(type, baseClass, typeProxy, isloaded, proxyHydra);
        //}

        void BuildNotifyMethod(TypeBuilder type, MethodInfo getChangeTracker, FieldInfo propertyChangedField)
        {
            var methodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig;
            MethodBuilder method = type.DefineMethod("NotifyChange", methodAttributes);

            //// Preparing Reflection instances
            //MethodInfo method1 = baseClass.GetMethod("get_ChangeTracker",
            //    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            //    null,
            //    new Type[] { },
            //    null
            //    );

            MethodInfo method2 = typeof(IChangeTracker).GetMethod("Track",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(String), typeof(Object) },
                null
                );

            //FieldInfo field3 = baseClass.GetField("PropertyChanged", BindingFlags.Public | BindingFlags.NonPublic);
            ConstructorInfo ctor4 = typeof(PropertyChangedEventArgs).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(String) },
                null
                );

            MethodInfo method5 = typeof(PropertyChangedEventHandler).GetMethod("Invoke",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(Object), typeof(PropertyChangedEventArgs) },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(void));
            // Adding parameters
            method.SetParameters(typeof(String), typeof(Object));
            // Parameter propName
            ParameterBuilder propName = method.DefineParameter(1, ParameterAttributes.None, "propName");
            // Parameter value
            ParameterBuilder value = method.DefineParameter(2, ParameterAttributes.None, "value");
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder csLocalBool = gen.DeclareLocal(typeof(Boolean));
            // Preparing labels
            Label label49 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, getChangeTracker);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Callvirt, method2);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, propertyChangedField);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Brtrue_S, label49);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, propertyChangedField);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Newobj, ctor4);
            gen.Emit(OpCodes.Callvirt, method5);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, propertyChangedField);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, "IsDirty");
            gen.Emit(OpCodes.Newobj, ctor4);
            gen.Emit(OpCodes.Callvirt, method5);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.MarkLabel(label49);
            gen.Emit(OpCodes.Ret);


        }


        MethodBuilder CreateChangeTrackerProperty(TypeBuilder type, FieldInfo changeTrackerField)
        {
            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            MethodBuilder method = type.DefineMethod("get_ChangeTracker", methodAttributes);
            
            method.SetReturnType(typeof(IChangeTracker));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder localBuilder = gen.DeclareLocal(typeof(IChangeTracker));
            // Preparing labels
            Label label10 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, changeTrackerField);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label10);
            gen.MarkLabel(label10);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;

        }

        void CreateIsDirtyProperty(TypeBuilder type, Type baseClass)
        {
            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            MethodBuilder method = type.DefineMethod("get_IsDirty", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = baseClass.GetMethod("get_ChangeTracker", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { },
                null
                );

            MethodInfo method2 = typeof(IChangeTracker).GetMethod(
                "get_HasChanges",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { },
                null
                );

            // Setting return type
            method.SetReturnType(typeof(Boolean));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder cs1000 = gen.DeclareLocal(typeof(Boolean));
            LocalBuilder cs1001 = gen.DeclareLocal(typeof(Boolean));
            // Preparing labels
            Label label29 = gen.DefineLabel();
            Label label34 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Brtrue_S, label29);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Callvirt, method2);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label34);
            gen.MarkLabel(label29);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label34);
            gen.MarkLabel(label34);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);

        }

        void CreateVersionProperty(TypeBuilder type, Type baseClass)
        {
            FieldBuilder versionField = type.DefineField("version", typeof(IChangeTracker), FieldAttributes.Private);

            //get_Version
            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            MethodBuilder getMethod = type.DefineMethod("get_Version", methodAttributes);
            getMethod.SetReturnType(typeof(Int64));
            ILGenerator getGenerator = getMethod.GetILGenerator();
            LocalBuilder cs1000 = getGenerator.DeclareLocal(typeof(Int64));
            Label label10 = getGenerator.DefineLabel();
            getGenerator.Emit(OpCodes.Nop);
            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Ldfld, versionField);
            getGenerator.Emit(OpCodes.Stloc_0);
            getGenerator.Emit(OpCodes.Br_S, label10);
            getGenerator.MarkLabel(label10);
            getGenerator.Emit(OpCodes.Ldloc_0);
            getGenerator.Emit(OpCodes.Ret);

            //set_Version

            MethodBuilder setMethod = type.DefineMethod("set_Version", methodAttributes);
            // Preparing Reflection instances

            FieldInfo field2 = baseClass.GetField("PropertyChanged", BindingFlags.Public | BindingFlags.NonPublic);
            ConstructorInfo ctor3 = typeof(PropertyChangedEventArgs).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(String)
            },
                null
                );
            MethodInfo method4 = typeof(PropertyChangedEventHandler).GetMethod(
                "Invoke",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(PropertyChangedEventArgs)
            },
                null
                );
            // Setting return type
            setMethod.SetReturnType(typeof(Void));
            // Adding parameters
            setMethod.SetParameters(
                typeof(Int64)
                );
            // Parameter value
            ParameterBuilder value = setMethod.DefineParameter(1, ParameterAttributes.None, "value");
            ILGenerator gen = setMethod.GetILGenerator();
            // Preparing locals
            LocalBuilder cs404 = gen.DeclareLocal(typeof(Boolean));
            // Preparing labels
            Label label46 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, versionField);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, field2);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Brtrue_S, label46);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, field2);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, "Version");
            gen.Emit(OpCodes.Newobj, ctor3);
            gen.Emit(OpCodes.Callvirt, method4);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.MarkLabel(label46);
            gen.Emit(OpCodes.Ret);

        }

        protected override MethodBuilder OverrideGetProperty(PropertyInfo PropertyAccessor, Type baseType, TypeBuilder type, MethodBuilder loadMeMethodBuilder)
        {
            System.Reflection.MethodAttributes methodAttributes = MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.HideBySig;

            string methodName = "get_" + PropertyAccessor.Name;
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
            method.SetReturnType(PropertyAccessor.PropertyType);
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder localBuilder = gen.DeclareLocal(PropertyAccessor.PropertyType);
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

        protected override MethodBuilder OverrideSetProperty(PropertyInfo PropertyAccessor, Type baseType, TypeBuilder type, MethodBuilder loadMeMethodBuilder)
        {
            System.Reflection.MethodAttributes methodAttributes = MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.HideBySig;

            string methodName = "set_" + PropertyAccessor.Name;
            MethodBuilder method = type.DefineMethod(methodName, methodAttributes);
            // Preparing Reflection instances

            MethodInfo baseMethod = baseType.GetMethod(methodName);

            // Setting return type
            method.SetReturnType(typeof(void));
            // Adding parameters
            method.SetParameters(PropertyAccessor.PropertyType);
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

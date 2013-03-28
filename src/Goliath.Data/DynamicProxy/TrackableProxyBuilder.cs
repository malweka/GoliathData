using System;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using Goliath.Data.Entity;

namespace Goliath.Data.DynamicProxy
{
    public class TrackableProxyBuilder : ProxyBuilder
    {
        protected override void AddInterfaceImplementation(TypeBuilder typeBuilder)
        {
            base.AddInterfaceImplementation(typeBuilder);
            typeBuilder.AddInterfaceImplementation(typeof(ITrackable));
            typeBuilder.AddInterfaceImplementation(typeof(INotifyPropertyChanged));
        }

        MethodBuilder BuildMethodGetInitialValues(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            MethodAttributes methodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static;
            MethodBuilder method = type.DefineMethod("GetInitialValues", methodAttributes);
            // Preparing Reflection instances
            ConstructorInfo ctor1 = typeof(System.Collections.Generic.Dictionary<,>).MakeGenericType(typeof(String), typeof(Object)).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);

            MethodInfo method2 = typeof(System.Collections.Generic.IDictionary<,>)
                .MakeGenericType(typeof(String), typeof(Object)).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new Type[] { typeof(string), typeof(object) },
            null
        );
            // Setting return type
            method.SetReturnType(typeof(System.Collections.Generic.IDictionary<,>).MakeGenericType(typeof(String), typeof(Object)));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder initialValues = gen.DeclareLocal(typeof(System.Collections.Generic.IDictionary<,>).MakeGenericType(typeof(String), typeof(Object)));
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(System.Collections.Generic.IDictionary<,>).MakeGenericType(typeof(String), typeof(Object)));
            // Preparing labels
            Label label37 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Newobj, ctor1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldstr, "Name");
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Callvirt, method2);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldstr, "Age");
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Callvirt, method2);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Br_S, label37);
            gen.MarkLabel(label37);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        protected override MethodBuilder BuildConstructor(TypeBuilder typeBuilder, Type baseClass, FieldBuilder typeProxy,
            FieldBuilder isloaded, FieldBuilder proxyHydra)
        {
            FieldBuilder changeTrackField = typeBuilder.DefineField("_changeTracker", typeof(IChangeTracker), FieldAttributes.Private);
            FieldBuilder propertyChangedField = CreatePropertyChangedEvent(typeBuilder); //typeBuilder.DefineField("PropertyChanged", typeof(PropertyChangedEventHandler), FieldAttributes.Private);

            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;
            MethodBuilder method = typeBuilder.DefineMethod(".ctor", methodAttributes);

            ConstructorInfo ctor1 = baseClass.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { },
                null
                );

            MethodInfo getInitialValuesMethodBuilder = BuildMethodGetInitialValues(typeBuilder);

            ConstructorInfo ctor6 = typeof(System.Func<>)
                .MakeGenericType(typeof(System.Collections.Generic.IDictionary<,>)
                .MakeGenericType(typeof(String), typeof(Object))).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(Object), typeof(IntPtr) }
                    , null);


            ConstructorInfo ctor7 = typeof(ChangeTracker)
                .GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                new Type[] { typeof(System.Func<>).MakeGenericType(typeof(System.Collections.Generic.IDictionary<,>).MakeGenericType(typeof(String), typeof(Object))) },
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
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ldftn, getInitialValuesMethodBuilder);
            gen.Emit(OpCodes.Newobj, ctor6);
            gen.Emit(OpCodes.Newobj, ctor7);
            gen.Emit(OpCodes.Stfld, changeTrackField);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);

            var getChangeTrackerMethod = CreateChangeTrackerProperty(typeBuilder, changeTrackField);
            BuildNotifyMethod(typeBuilder, getChangeTrackerMethod, propertyChangedField);
            CreateIsDirtyProperty(typeBuilder, getChangeTrackerMethod);
            CreateVersionProperty(typeBuilder, propertyChangedField);

            return method;
        }

        private MethodBuilder notifyChangeMethod;
        void BuildNotifyMethod(TypeBuilder type, MethodInfo getChangeTracker, FieldInfo propertyChangedField)
        {
            var methodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig;
            notifyChangeMethod = type.DefineMethod("NotifyChange", methodAttributes);

            MethodInfo method2 = typeof(IChangeTracker).GetMethod("Track",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(String), typeof(Object) },
                null
                );

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
            notifyChangeMethod.SetReturnType(typeof(void));
            // Adding parameters
            notifyChangeMethod.SetParameters(typeof(String), typeof(Object));
            // Parameter propName
            ParameterBuilder propName = notifyChangeMethod.DefineParameter(1, ParameterAttributes.None, "propName");
            // Parameter value
            ParameterBuilder value = notifyChangeMethod.DefineParameter(2, ParameterAttributes.None, "value");
            ILGenerator gen = notifyChangeMethod.GetILGenerator();
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
            ILGenerator gen = method.GetILGenerator();
            LocalBuilder localBuilder = gen.DeclareLocal(typeof(IChangeTracker));
            Label label10 = gen.DefineLabel();
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, changeTrackerField);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label10);
            gen.MarkLabel(label10);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            return method;
        }

        void CreateIsDirtyProperty(TypeBuilder type, MethodInfo getChangeTrackerMethod)
        {
            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            MethodBuilder method = type.DefineMethod("get_IsDirty", methodAttributes);

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
            gen.Emit(OpCodes.Call, getChangeTrackerMethod);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Brtrue_S, label29);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, getChangeTrackerMethod);
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

        void CreateVersionProperty(TypeBuilder type, FieldInfo propertyChangedField)
        {
            FieldBuilder versionField = type.DefineField("version", typeof(Int64), FieldAttributes.Private);

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
            ConstructorInfo ctor3 = typeof(PropertyChangedEventArgs).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(String) },
                null
                );
            MethodInfo method4 = typeof(PropertyChangedEventHandler).GetMethod("Invoke",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(Object), typeof(PropertyChangedEventArgs) },
                null
                );
            // Setting return type
            setMethod.SetReturnType(typeof(void));
            // Adding parameters
            setMethod.SetParameters(
                typeof(Int64)
                );
            ParameterBuilder value = setMethod.DefineParameter(1, ParameterAttributes.None, "value");
            ILGenerator gen = setMethod.GetILGenerator();
            LocalBuilder flag = gen.DeclareLocal(typeof(Boolean));
            Label label46 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, versionField);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, propertyChangedField);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Brtrue_S, label46);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, propertyChangedField);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, "Version");
            gen.Emit(OpCodes.Newobj, ctor3);
            gen.Emit(OpCodes.Callvirt, method4);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, propertyChangedField);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, "IsDirty");
            gen.Emit(OpCodes.Newobj, ctor3);
            gen.Emit(OpCodes.Callvirt, method4);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.MarkLabel(label46);
            gen.Emit(OpCodes.Ret);

        }

        //create add remove method from http://grahammurray.wordpress.com/tag/reflection-emit/
        FieldBuilder CreatePropertyChangedEvent(TypeBuilder type)
        {
            FieldBuilder eventField = type.DefineField("PropertyChanged", typeof(PropertyChangedEventHandler), FieldAttributes.Private);
            EventBuilder eventBuilder = type.DefineEvent("PropertyChanged", EventAttributes.None, typeof(PropertyChangedEventHandler));
            eventBuilder.SetAddOnMethod(CreateAddRemoveMethod(type, eventField, true));
            eventBuilder.SetRemoveOnMethod(CreateAddRemoveMethod(type, eventField, false));

            return eventField;
        }

        //create add remove method from http://grahammurray.wordpress.com/tag/reflection-emit/
        MethodBuilder CreateAddRemoveMethod(TypeBuilder type, FieldBuilder propertyChangedField, bool isAdd)
        {
            var methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual |
                                   MethodAttributes.Final;
            string prefix = "remove_";
            string delegateAction = "Remove";

            if (isAdd)
            {
                prefix = "add_";
                delegateAction = "Combine";
            }

            MethodBuilder addremoveMethod = type.DefineMethod(prefix + "PropertyChanged", methodAttributes, null, new[] { typeof(PropertyChangedEventHandler) });
            MethodImplAttributes eventMethodFlags = MethodImplAttributes.Managed | MethodImplAttributes.Synchronized;
            addremoveMethod.SetImplementationFlags(eventMethodFlags);

            ILGenerator gen = addremoveMethod.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, propertyChangedField);
            gen.Emit(OpCodes.Ldarg_1);
            gen.EmitCall(OpCodes.Call, typeof(Delegate).GetMethod(delegateAction, new[] { typeof(Delegate), typeof(Delegate) }), null);
            gen.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
            gen.Emit(OpCodes.Stfld, propertyChangedField);
            gen.Emit(OpCodes.Ret);

            MethodInfo intAddRemoveMethod = typeof(INotifyPropertyChanged).GetMethod(prefix + "PropertyChanged"); type.DefineMethodOverride(
            addremoveMethod, intAddRemoveMethod);

            return addremoveMethod;
        }

        /// <summary>
        /// Overrides the set property.
        /// </summary>
        /// <param name="propertyAccessor">The property accessor.</param>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="type">The type.</param>
        /// <param name="loadMeMethodBuilder">The load me method builder.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">NotifyChange method not yet build</exception>
        protected override MethodBuilder OverrideSetProperty(PropertyInfo propertyAccessor, Type baseType, TypeBuilder type, MethodBuilder loadMeMethodBuilder)
        {
            if (notifyChangeMethod == null)
                throw new InvalidOperationException("NotifyChange method not yet build");

            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;

            string setMethodName = "set_" + propertyAccessor.Name;
            string getMethodName = "get_" + propertyAccessor.Name;

            MethodBuilder method = type.DefineMethod(setMethodName, methodAttributes);
            // Preparing Reflection instances

            MethodInfo baseSetMethod = baseType.GetMethod(setMethodName);
            MethodInfo baseGetMethod = baseType.GetMethod(getMethodName);

            MethodInfo objectEqualsMethod = typeof(Object).GetMethod("Equals",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(Object), typeof(Object) },
                null
                );

            method.SetReturnType(typeof(void));
            // Adding parameters
            method.SetParameters(propertyAccessor.PropertyType);
            // Parameter value
            ParameterBuilder value = method.DefineParameter(1, ParameterAttributes.None, "value");
            ILGenerator gen = method.GetILGenerator();
            LocalBuilder flag1 = gen.DeclareLocal(typeof(Boolean));
            Label label47 = gen.DefineLabel();
            // Writing body
            //gen.Emit(OpCodes.Nop);
            //gen.Emit(OpCodes.Ldarg_0);
            //gen.Emit(OpCodes.Call, loadMeMethodBuilder);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, baseGetMethod);
            gen.Emit(OpCodes.Call, objectEqualsMethod);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Brtrue_S, label47);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, baseSetMethod);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, propertyAccessor.Name);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, notifyChangeMethod);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.MarkLabel(label47);
            gen.Emit(OpCodes.Ret);

            return method;
        }
    }
}

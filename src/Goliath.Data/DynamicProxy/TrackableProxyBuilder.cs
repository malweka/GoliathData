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
            typeBuilder.AddInterfaceImplementation(typeof(ICustomTypeDescriptor));
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
            CreateIsDirtyProperty(typeBuilder, getChangeTrackerMethod);
            CreateVersionProperty(typeBuilder, propertyChangedField);

            BuildComponentModelICustomTypeDescriptorGetAttributes(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetClassName(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetComponentName(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetConverter(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetDefaultEvent(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetDefaultProperty(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetEditor(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetEvents(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetEventsAttributes(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetProperties(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetPropertiesAttributes(typeBuilder);
            BuildComponentModelICustomTypeDescriptorGetPropertyOwner(typeBuilder);


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

        #region implement ICustomTypeDescriptor
        MethodAttributes customDescriptorAttributes = System.Reflection.MethodAttributes.Public
                | System.Reflection.MethodAttributes.Virtual
                | System.Reflection.MethodAttributes.Final
                | System.Reflection.MethodAttributes.HideBySig
                | System.Reflection.MethodAttributes.NewSlot;

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetAttributes(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetAttributes", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(TypeDescriptor).GetMethod(
                "GetAttributes",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(Boolean)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(AttributeCollection));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(AttributeCollection));
            // Preparing labels
            Label label11 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label11);
            gen.MarkLabel(label11);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetClassName(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetClassName", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(TypeDescriptor).GetMethod(
                "GetClassName",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(Boolean)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(String));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(String));
            // Preparing labels
            Label label11 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label11);
            gen.MarkLabel(label11);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetComponentName(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetComponentName", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(TypeDescriptor).GetMethod(
                "GetComponentName",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(Boolean)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(String));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(String));
            // Preparing labels
            Label label11 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label11);
            gen.MarkLabel(label11);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetConverter(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetConverter", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(TypeDescriptor).GetMethod(
                "GetConverter",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(Boolean)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(TypeConverter));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(TypeConverter));
            // Preparing labels
            Label label11 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label11);
            gen.MarkLabel(label11);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetDefaultEvent(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetDefaultEvent", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(TypeDescriptor).GetMethod(
                "GetDefaultEvent",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(Boolean)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(EventDescriptor));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(EventDescriptor));
            // Preparing labels
            Label label11 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label11);
            gen.MarkLabel(label11);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetDefaultProperty(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetDefaultProperty", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(TypeDescriptor).GetMethod(
                "GetDefaultProperty",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(Boolean)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(PropertyDescriptor));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(PropertyDescriptor));
            // Preparing labels
            Label label11 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label11);
            gen.MarkLabel(label11);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetEditor(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetEditor", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(TypeDescriptor).GetMethod(
                "GetEditor",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(Type),
            typeof(Boolean)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(Object));
            // Adding parameters
            method.SetParameters(
                typeof(Type)
                );
            // Parameter editorBaseType
            ParameterBuilder editorBaseType = method.DefineParameter(1, ParameterAttributes.None, "editorBaseType");
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(Object));
            // Preparing labels
            Label label12 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label12);
            gen.MarkLabel(label12);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetEvents(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetEvents", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(TypeDescriptor).GetMethod(
                "GetEvents",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(Boolean)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(EventDescriptorCollection));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(EventDescriptorCollection));
            // Preparing labels
            Label label11 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label11);
            gen.MarkLabel(label11);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetEventsAttributes(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetEvents", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(TypeDescriptor).GetMethod(
                "GetEvents",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(Attribute[]),
            typeof(Boolean)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(EventDescriptorCollection));
            // Adding parameters
            method.SetParameters(
                typeof(Attribute[])
                );
            // Parameter attributes
            ParameterBuilder attributes = method.DefineParameter(1, ParameterAttributes.None, "attributes");
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(EventDescriptorCollection));
            // Preparing labels
            Label label12 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label12);
            gen.MarkLabel(label12);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetProperties(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetProperties", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(Object).GetMethod(
                "GetType",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            },
                null
                );
            MethodInfo method2 = typeof(TypeDescriptor).GetMethod(
                "GetProperties",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Type)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(PropertyDescriptorCollection));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(PropertyDescriptorCollection));
            // Preparing labels
            Label label15 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Call, method2);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label15);
            gen.MarkLabel(label15);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;

        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetPropertiesAttributes(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetProperties", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(Object).GetMethod(
                "GetType",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            },
                null
                );
            MethodInfo method2 = typeof(TypeDescriptor).GetMethod(
                "GetProperties",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Type),
            typeof(Attribute[])
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(PropertyDescriptorCollection));
            // Adding parameters
            method.SetParameters(
                typeof(Attribute[])
                );
            // Parameter attributes
            ParameterBuilder attributes = method.DefineParameter(1, ParameterAttributes.None, "attributes");
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS11000 = gen.DeclareLocal(typeof(PropertyDescriptorCollection));
            // Preparing labels
            Label label16 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, method2);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label16);
            gen.MarkLabel(label16);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;

        }

        MethodBuilder BuildComponentModelICustomTypeDescriptorGetPropertyOwner(TypeBuilder type)
        {
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes = customDescriptorAttributes;
            MethodBuilder method = type.DefineMethod("GetPropertyOwner", methodAttributes);
            // Preparing Reflection instances
            // Setting return type
            method.SetReturnType(typeof(Object));
            // Adding parameters
            method.SetParameters(
                typeof(PropertyDescriptor)
                );
            // Parameter pd
            ParameterBuilder pd = method.DefineParameter(1, ParameterAttributes.None, "pd");
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder CS10000 = gen.DeclareLocal(typeof(Object));
            // Preparing labels
            Label label5 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label5);
            gen.MarkLabel(label5);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        #endregion
    }
}

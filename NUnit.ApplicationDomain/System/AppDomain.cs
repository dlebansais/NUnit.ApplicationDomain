﻿#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System;

using global::System;
using global::System.Collections;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Reflection;
using global::System.Runtime.Loader;
using global::System.Text.Json;
using NUnit.ApplicationDomain.System.Security;
using NUnit.ApplicationDomain.System.Security.Policy;

/// <summary>
/// A fake AppDomain class for .NET Core.
/// </summary>
internal class AppDomain : MarshalByRefObject, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDomain"/> class.
    /// </summary>
    private AppDomain()
    {
        FriendlyName = string.Empty;
        Info = new AppDomainSetup();
        Context = AssemblyLoadContext.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDomain"/> class.
    /// </summary>
    /// <param name="friendlyName">The domain friendly name.</param>
    /// <param name="securityInfo">The domain security info.</param>
    /// <param name="info">The domain construction info.</param>
    /// <param name="grantSet">The domain permission set.</param>
    /// <param name="fullTrustAssemblies">The list of trusted assemblies.</param>
    private AppDomain(string friendlyName, Evidence? securityInfo, AppDomainSetup info, PermissionSet grantSet, params StrongName[] fullTrustAssemblies)
    {
        Info = info;
        Context = new AppDomainAssemblyLoadContext($"#{DomainCount}", GetType().Assembly.Location);
        DomainCount++;

        if (TryClone(this, out MarshalByRefObject? DomainClone))
            RegisteredDomains.Add(Context, DomainClone!);

        FriendlyName = $"{friendlyName} ({Context.Name})";

        Context.Resolving += OnResolving;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDomain"/> class.
    /// </summary>
    /// <param name="friendlyName">The domain friendly name.</param>
    public AppDomain(string friendlyName)
    {
        FriendlyName = friendlyName;
        Info = new AppDomainSetup();
        Context = AssemblyLoadContext.Default;
    }

    private static Dictionary<AssemblyLoadContext, object> RegisteredDomains
    {
        get
        {
            object? SharedRegisteredDomains = global::System.AppDomain.CurrentDomain.GetData("RegisteredDomains");
            if (SharedRegisteredDomains is null)
                global::System.AppDomain.CurrentDomain.SetData("RegisteredDomains", new Dictionary<AssemblyLoadContext, object>() { { AssemblyLoadContext.Default, new AppDomain() } });

            return (Dictionary<AssemblyLoadContext, object>)global::System.AppDomain.CurrentDomain.GetData("RegisteredDomains")!;
        }
    }

    /// <summary>
    /// Gets the current domain.
    /// </summary>
    public static AppDomain CurrentDomain
    {
        get
        {
            AssemblyLoadContext? CurrentContext = AssemblyLoadContext.GetLoadContext(Assembly.GetCallingAssembly());
            if (CurrentContext is not null && RegisteredDomains.TryGetValue(CurrentContext, out object? Value) && Value is AppDomain Domain)
                return Domain;
            else
                return null!;
        }
    }

    /// <summary>
    /// Gets the friendly name.
    /// </summary>
    public string FriendlyName { get; }

    /// <summary>
    /// Creates a new AppDomain.
    /// </summary>
    /// <param name="friendlyName">The domain friendly name.</param>
    /// <param name="securityInfo">The domain security info.</param>
    /// <param name="info">The domain construction info.</param>
    /// <param name="grantSet">The domain permission set.</param>
    /// <param name="fullTrustAssemblies">The list of trusted assemblies.</param>
    internal static AppDomain CreateDomain(string friendlyName, Evidence? securityInfo, AppDomainSetup info, PermissionSet grantSet, params StrongName[] fullTrustAssemblies)
    {
        return new AppDomain(friendlyName, securityInfo, info, grantSet, fullTrustAssemblies);
    }

    /// <summary>
    /// Creates a new instance of a type in the domain.
    /// </summary>
    /// <param name="assemblyPath">The path to the assembly containing the type.</param>
    /// <param name="typeName">The type name.</param>
    /// <param name="usePublicConstructor"><see langword="true"/> to use a public constructor.</param>
    /// <param name="args">The constructor arguments.</param>
    public object? CreateInstanceAndUnwrap(string assemblyPath, string typeName, bool usePublicConstructor, params object[] args)
    {
        Assembly LoadedAssembly = Context.LoadFromAssemblyPath(assemblyPath);
        object? Result = LoadedAssembly.CreateInstance(typeName, ignoreCase: false, BindingFlags.CreateInstance | BindingFlags.Instance | (usePublicConstructor ? BindingFlags.Public : BindingFlags.NonPublic), binder: null, args, culture: null, activationAttributes: null);
        return Result;
    }

    /// <summary>
    /// Loads an assembly in the domain.
    /// </summary>
    /// <param name="assemblyPath">The path to the assembly.</param>
    public Assembly Load(string assemblyPath)
    {
        return Context.LoadFromAssemblyPath(assemblyPath);
    }

    /// <summary>
    /// Unloads a domain.
    /// </summary>
    /// <param name="domain">The domain to unload.</param>
    public static void Unload(AppDomain domain)
    {
        domain.Dispose();
    }

    /// <summary>
    /// Gets domain-specific data.
    /// </summary>
    /// <param name="name">The key to the data.</param>
    public object? GetData(string name)
    {
        if (RegisteredDomains.TryGetValue(Context, out object? Value))
        {
            Dictionary<string, object?> DomainData = (Dictionary<string, object?>?)Value!.GetType().GetProperty("Data", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(Value)!;
            if (DomainData.TryGetValue(name, out var data))
                return data;
            else
                return null;
        }
        else
            return null;
    }

    /// <summary>
    /// Sets domain-specific data.
    /// </summary>
    /// <param name="name">The key to the data.</param>
    /// <param name="data">The data.</param>
    public void SetData(string name, object? data)
    {
        if (RegisteredDomains.TryGetValue(Context, out object? Value))
        {
            Dictionary<string, object?> DomainData = (Dictionary<string, object?>?)Value!.GetType().GetProperty("Data", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(Value)!;
            if (!DomainData.TryAdd(name, data))
                DomainData[name] = data;
        }
    }

    private Assembly? RaiseAssemblyResolve(string name)
    {
        return AssemblyResolve?.Invoke(this, new ResolveEventArgs(name));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Context.Resolving -= OnResolving;
        RegisteredDomains.Remove(Context);

        Context.Unload();
    }

    private Assembly? OnResolving(AssemblyLoadContext context, AssemblyName assemblyRef)
    {
        return RaiseAssemblyResolve(assemblyRef.FullName);
    }

    /// <summary>
    /// Tries to clone an object in the domain.
    /// </summary>
    /// <param name="obj">The object to clone.</param>
    /// <param name="clone">The cloned object upon return.</param>
    public bool TryClone(object? obj, out object? clone)
    {
        clone = null;

        if (obj is null)
            return true;

        Type ObjType = obj.GetType();

        if (ObjType.IsPrimitive)
        {
            clone = obj;
            return true;
        }
        else if (ObjType == typeof(string))
        {
            clone = obj;
            return true;
        }
        else if (ObjType.IsAssignableTo(typeof(MarshalByRefObject)))
        {
            MarshalByRefObject ObjValue = (MarshalByRefObject)obj;
            if (TryClone(ObjValue, out MarshalByRefObject? CloneValue))
            {
                clone = CloneValue;
                return true;
            }
        }
        else if (ObjType.IsArray)
        {
            if (TryGetTypeInDomain(ObjType, out Type? CloneListType))
            {
                if (TryCreateEmptyInstance(CloneListType!, obj, out clone))
                {
                    Array ListFieldValue = (Array)obj;
                    Array CloneListFieldValue = (Array)clone!;

                    for (int i = 0; i < ListFieldValue.Length; i++)
                    {
                        if (TryClone(ListFieldValue.GetValue(i), out object? CloneItem))
                            CloneListFieldValue.SetValue(CloneItem, i);
                        else
                            break;
                    }

                    if (CloneListFieldValue.Length == ListFieldValue.Length)
                    {
                        clone = CloneListFieldValue;
                        return true;
                    }
                }
            }
        }
        else if (ObjType.IsAssignableTo(typeof(IList)))
        {
            if (TryGetTypeInDomain(ObjType, out Type? CloneListType))
            {
                if (TryCreateEmptyInstance(CloneListType!, obj, out clone))
                {
                    IList ListFieldValue = (IList)obj;
                    IList CloneListFieldValue = (IList)clone!;

                    foreach (object Item in ListFieldValue)
                        if (TryClone(Item, out object? CloneItem))
                            CloneListFieldValue.Add(CloneItem);
                        else
                            break;

                    if (CloneListFieldValue.Count == ListFieldValue.Count)
                    {
                        clone = CloneListFieldValue;
                        return true;
                    }
                }
            }
        }
        else if (ObjType.IsAssignableTo(typeof(IDictionary)))
        {
            if (TryGetTypeInDomain(ObjType, out Type? CloneDictionaryType))
            {
                if (TryCreateEmptyInstance(CloneDictionaryType!, obj, out clone))
                {
                    IDictionary DictionaryFieldValue = (IDictionary)obj;
                    IDictionary CloneDictionaryFieldValue = (IDictionary)clone!;

                    foreach (object Key in DictionaryFieldValue.Keys)
                        if (TryClone(Key, out object? CloneKey) && TryClone(DictionaryFieldValue[Key], out object? CloneValue))
                            CloneDictionaryFieldValue.Add(CloneKey!, CloneValue);
                        else
                            break;

                    if (CloneDictionaryFieldValue.Count == DictionaryFieldValue.Count)
                    {
                        clone = CloneDictionaryFieldValue;
                        return true;
                    }
                }
            }
        }
        else if (ObjType.IsAssignableTo(typeof(MethodInfo)))
        {
            MethodInfo ObjMethod = (MethodInfo)obj;
            Type DeclaringType = ObjMethod.DeclaringType!;
            var Context = AssemblyLoadContext.GetLoadContext(DeclaringType.Assembly);

            if (TryGetTypeInDomain(DeclaringType, out Type? CloneDeclaringType))
            {
                var CloneContext = AssemblyLoadContext.GetLoadContext(CloneDeclaringType!.Assembly);
                MethodInfo[] ClonedMethods = CloneDeclaringType.GetMethods();

                foreach (MethodInfo Method in ClonedMethods)
                    if (Method.Name == ObjMethod.Name)
                    {
                        clone = Method;
                        return true;
                    }
            }
        }
        else if (ObjType.IsAssignableTo(typeof(Type)))
        {
            if (TryGetTypeInDomain((Type)obj, out Type? CloneObjType))
            {
                clone = CloneObjType;
                return true;
            }
        }
        else if (ObjType.IsAssignableTo(typeof(AssemblyLoadContext)))
        {
            clone = obj;
            return true;
        }
        else if (Attribute.IsDefined(ObjType, typeof(SerializableAttribute)))
        {
            if (TryGetTypeInDomain(ObjType, out Type? CloneDictionaryType))
            {
                string JSonString = JsonSerializer.Serialize(obj);
                object? DeserializedObj = JsonSerializer.Deserialize(JSonString, CloneDictionaryType!);

                if (DeserializedObj is not null)
                {
                    clone = DeserializedObj;
                    return true;
                }
            }
        }

        clone = null;
        return false;
    }

    /// <summary>
    /// Tries to clone an instance of <see cref="MarshalByRefObject" /> in the domain.
    /// </summary>
    /// <param name="obj">The object to clone.</param>
    /// <param name="clone">The cloned object upon return.</param>
    public bool TryClone(MarshalByRefObject obj, out MarshalByRefObject? clone)
    {
        clone = null;

        Type SourceType = obj.GetType();
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        FieldInfo[] SourceFields = SourceType.GetFields(Flags);

        if (!TryCreateEmptyInstance(SourceType, obj, out object? Instance))
            return false;

        clone = (MarshalByRefObject)Instance!;
        Type DestinationType = clone.GetType();
        FieldInfo[] DestinationFields = DestinationType.GetFields(Flags);

        for (int i = 0; i < DestinationFields.Length; i++)
        {
            FieldInfo SourceField = SourceFields[i];
            FieldInfo DestinationField = DestinationFields[i];

            if (SourceField.Name.EndsWith("__BackingField", StringComparison.Ordinal))
                continue;

            Type FieldType = SourceField.FieldType;
            object? FieldValue = SourceField.GetValue(obj);

            if (TryClone(FieldValue, out object? CloneValue))
                DestinationField.SetValue(clone, CloneValue);
            else
                return false;
        }

        return true;
    }

    /// <summary>
    /// Tries to gets a type in the domain.
    /// </summary>
    /// <param name="type">The type outside the domain.</param>
    /// <param name="typeInDomain">The type in the domain upon return.</param>
    public bool TryGetTypeInDomain(Type type, out Type? typeInDomain)
    {
        if (type.IsPrimitive)
        {
            typeInDomain = type;
            return true;
        }
        else if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            if (TryGetTypeInDomain(type.GetGenericTypeDefinition(), out Type? CloneGenericTypeDefinition))
            {
                Type[] GenericArguments = type.GetGenericArguments();
                List<Type> CloneGenericArguments = new();

                for (int i = 0; i < GenericArguments.Length; i++)
                    if (TryGetTypeInDomain(GenericArguments[i], out Type? ArgumentType))
                        CloneGenericArguments.Add(ArgumentType!);

                if (CloneGenericArguments.Count == GenericArguments.Length)
                {
                    typeInDomain = CloneGenericTypeDefinition!.MakeGenericType(CloneGenericArguments.ToArray());
                    return true;
                }
            }
        }
        else if (type.IsArray)
        {
            if (TryGetTypeInDomain(type.GetElementType()!, out Type? ElementTypeInDomain))
            {
                typeInDomain = ElementTypeInDomain!.MakeArrayType();
                return true;
            }
        }
        else
        {
            string AssemblyLocation = type.Assembly.Location;
            string TypeFullName = type.FullName!;

            Assembly AssemblyInDomain;

            string SystemAssemblyLocation = typeof(IList).Assembly.Location;

            if (AssemblyLocation == SystemAssemblyLocation)
                AssemblyInDomain = type.Assembly;
            else
                AssemblyInDomain = Context.LoadFromAssemblyPath(AssemblyLocation);

            typeInDomain = AssemblyInDomain.GetType(TypeFullName)!;
            return true;
        }

        typeInDomain = null;
        return false;
    }

    /// <summary>
    /// Tries to create an empty instance of a type in the domain.
    /// </summary>
    /// <param name="type">The type to use when creating the instance.</param>
    /// <param name="source">The source object with properties for constructing the new instance.</param>
    /// <param name="instance">The new instance upon return.</param>
    private bool TryCreateEmptyInstance(Type type, object source, out object? instance)
    {
        if (type.IsArray)
        {
            Array SourceArray = (Array)source;
            instance = Array.CreateInstance(type.GetElementType()!, SourceArray.Length);
            return true;
        }

        if (FindContructorAndArgs(type, source, out bool UsePublicConstructor, out object[] Args))
            if (TryCreateEmptyInstanceWithConstructor(type, source, UsePublicConstructor, Args, out instance))
                return true;

        instance = null;
        return false;
    }

    private bool TryCreateEmptyInstanceWithConstructor(Type type, object source, bool usePublicConstructor, object[] args, out object? instance)
    {
        string AssemblyLocation = type.Assembly.Location;
        string TypeFullName = type.FullName!;

        string SystemAssemblyLocation = typeof(IList).Assembly.Location;
        Assembly AssemblyInDomain;

        if (AssemblyLocation == SystemAssemblyLocation)
            AssemblyInDomain = type.Assembly;
        else
            AssemblyInDomain = Context.LoadFromAssemblyPath(AssemblyLocation);

        BindingFlags Flags = BindingFlags.CreateInstance | BindingFlags.Instance | (usePublicConstructor ? BindingFlags.Public : BindingFlags.NonPublic);
        object? createdInstance = AssemblyInDomain.CreateInstance(TypeFullName, ignoreCase: false, Flags, binder: null, args, culture: null, activationAttributes: null);

        if (createdInstance is not null)
        {
            instance = createdInstance;
            return true;
        }

        instance = null;
        return false;
    }

    private bool FindContructorAndArgs(Type type, object source, out bool usePublicConstructor, out object[] args)
    {
        if (FindContructorAndArgs(type, BindingFlags.Public, source, out args))
        {
            usePublicConstructor = true;
            return true;
        }

        usePublicConstructor = false;

        if (FindContructorAndArgs(type, BindingFlags.NonPublic, source, out args))
            return true;

        return false;
    }

    private bool FindContructorAndArgs(Type type, BindingFlags publicFlag, object source, out object[] args)
    {
        args = Array.Empty<object>();

        List<ConstructorInfo> Constructors = type.GetConstructors(BindingFlags.Instance | publicFlag).ToList();
        Constructors.Sort(ByParameterCountAscending);

        foreach (ConstructorInfo Constructor in Constructors)
        {
            ParameterInfo[] Parameters = Constructor.GetParameters();
            List<PropertyInfo> Properties = new();

            foreach (ParameterInfo Parameter in Parameters)
            {
                if (Parameter.Name is string ParameterName)
                {
                    string AssociatedPropertyName = ParameterName.ToUpperInvariant();
                    if (type.GetProperty(AssociatedPropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) is PropertyInfo Property)
                        Properties.Add(Property);
                    else if (type.GetProperty(AssociatedPropertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase) is PropertyInfo NonPublicProperty)
                        Properties.Add(NonPublicProperty);
                }
            }

            if (Properties.Count == Parameters.Length)
            {
                List<object> Args = new();

                for (int i = 0; i < Properties.Count; i++)
                {
                    PropertyInfo Property = Properties[i];
                    if (TryClone(Property.GetValue(source), out object? ClonedProperty))
                        Args.Add(ClonedProperty!);
                }

                if (Args.Count == Parameters.Length)
                {
                    args = Args.ToArray();
                    return true;
                }
            }
        }

        return false;
    }

    private static int ByParameterCountAscending(ConstructorInfo c1, ConstructorInfo c2)
    {
        return c1.GetParameters().Length - c2.GetParameters().Length;
    }

    /// <summary>
    /// An event triggered when an assembly must be resolved.
    /// </summary>
#pragma warning disable CA1003 // Use generic event handler instances: Cannot be fixed.
    public event ResolveEventHandler? AssemblyResolve;
#pragma warning restore CA1003 // Use generic event handler instances

    private static int DomainCount;
    private AppDomainSetup Info;
    private AssemblyLoadContext Context;
    private Dictionary<string, object?> Data { get; } = new();
}
#endif

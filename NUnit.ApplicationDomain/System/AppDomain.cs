#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System;

using global::System;
using global::System.Collections.Generic;
using global::System.Reflection;
using global::System.Runtime.Loader;
using NUnit.ApplicationDomain.System.Security;
using NUnit.ApplicationDomain.System.Security.Policy;

public class AppDomain : IDisposable
{
    private static Dictionary<AssemblyLoadContext, AppDomain> RegisteredDomains;
    private static int DomainCount;
    private static AppDomain Default;

    public static AppDomain CurrentDomain
    {
        get
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            AssemblyLoadContext? context = AssemblyLoadContext.GetLoadContext(assembly);
            if (context is not null && RegisteredDomains.TryGetValue(context, out var domain))
                return domain;
            else
                return Default;
        }
    }

    static AppDomain()
    {
        RegisteredDomains = new Dictionary<AssemblyLoadContext, AppDomain>();
        Default = new AppDomain();
    }

    private AppDomain()
    {
        FriendlyName = string.Empty;
        Info = new AppDomainSetup();

        Context = AssemblyLoadContext.Default;
        RegisteredDomains.Add(Context, this);
    }

    private AppDomain(string friendlyName, Evidence? securityInfo, AppDomainSetup info, PermissionSet grantSet, params StrongName[] fullTrustAssemblies)
    {
        Info = info;
        Context = new AssemblyLoadContext($"#{DomainCount}", isCollectible: true);
        DomainCount++;

        RegisteredDomains.Add(Context, this);

        FriendlyName = $"{friendlyName} ({Context.Name})";

        Context.Resolving += OnResolving;
    }

    public string FriendlyName { get; }
    public AppDomainSetup Info { get; }

    private AssemblyLoadContext Context;
    private Dictionary<string, object?> Data = new();

    internal static AppDomain CreateDomain(string friendlyName, Evidence? securityInfo, AppDomainSetup info, PermissionSet grantSet, params StrongName[] fullTrustAssemblies)
    {
        return new AppDomain(friendlyName, securityInfo, info, grantSet, fullTrustAssemblies);
    }

    public object? CreateInstanceAndUnwrap(string assemblyPath, string typeName, bool usePublicConstructor, params object[] args)
    {
        Assembly LoadedAssembly = Context.LoadFromAssemblyPath(assemblyPath);
        object? Result = LoadedAssembly.CreateInstance(typeName, ignoreCase: false, BindingFlags.CreateInstance | BindingFlags.Instance | (usePublicConstructor ? BindingFlags.Public : BindingFlags.NonPublic), binder: null, args, culture: null, activationAttributes: null);
        return Result;
    }

    public Assembly Load(string assemblyPath)
    {
        return Context.LoadFromAssemblyPath(assemblyPath);
    }

    public static void Unload(AppDomain domain)
    {
        domain.Dispose();
    }

    public object? GetData(string name)
    {
        if (Data.TryGetValue(name, out var data))
            return data;
        else
            return null;
    }

    public void SetData(string name, object? data)
    {
        if (Data.ContainsKey(name))
            Data[name] = data;
        else
            Data.Add(name, data);
    }

    private Assembly? RaiseAssemblyResolve(string name)
    {
        return AssemblyResolve?.Invoke(this, new ResolveEventArgs(name));
    }

    public void Dispose()
    {
        Context.Unload();
    }

    private Assembly? OnResolving(AssemblyLoadContext context, AssemblyName assemblyRef)
    {
        return RaiseAssemblyResolve(assemblyRef.FullName);
    }

    public void PrintContextName()
    {
        // Console.WriteLine($"** Current context is {Context.Name}");
    }

    public static string GetLoadContext(Assembly assembly)
    {
        if (assembly.ReflectionOnly)
            return "reflection-only context";
        else if (assembly.IsDynamic)
            return "no context (Dynamic)";
        else if (assembly.Location is null)
            return "no context (via Load(byte[]))";
        else if (assembly.Location.StartsWith("CodeBase"))
            return "default context";
        else
            return "some context";
    }

    public event ResolveEventHandler? AssemblyResolve;
}
#endif

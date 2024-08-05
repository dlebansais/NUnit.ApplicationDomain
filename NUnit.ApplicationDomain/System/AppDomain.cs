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
        FriendlyName = friendlyName;
        Info = info;
        Console.WriteLine($"Creating AppDomain with name {friendlyName}, {RegisteredDomains.Count} created already");

        Context = new AssemblyLoadContext($"#{DomainCount}", isCollectible: true);
        DomainCount++;

        RegisteredDomains.Add(Context, this);

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

    public object? CreateInstanceAndUnwrap(AssemblyName assemblyRef, string typeName)
    {
        Assembly LoadedAssembly = Context.LoadFromAssemblyName(assemblyRef);
        object? Result = LoadedAssembly.CreateInstance(typeName);

        Console.WriteLine($"Creating instance of {typeName} in {Context.Name}");
        return Result;
    }

    public Assembly Load(AssemblyName assemblyRef)
    {
        Console.WriteLine($"Loading {assemblyRef} in {Context.Name}");

        return Context.LoadFromAssemblyName(assemblyRef);
    }

    public static void Unload(AppDomain domain)
    {
        Console.WriteLine($"Unloading {domain.Context.Name}");

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
        Console.WriteLine($"Resolving {assemblyRef} for {Context.Name}");

        return RaiseAssemblyResolve(assemblyRef.FullName);
    }

    public event ResolveEventHandler? AssemblyResolve;
}
#endif


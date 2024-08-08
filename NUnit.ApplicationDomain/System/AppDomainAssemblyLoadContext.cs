#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System;

using global::System.Reflection;
using global::System.Runtime.Loader;

public class AppDomainAssemblyLoadContext : AssemblyLoadContext
{
    public AppDomainAssemblyLoadContext(string name, string mainAssemblyToLoadPath) : base(name, isCollectible: true)
    {
        Resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
    }

    protected override Assembly? Load(AssemblyName name)
    {
        string? assemblyPath = Resolver.ResolveAssemblyToPath(name);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    private AssemblyDependencyResolver Resolver;
}

#endif

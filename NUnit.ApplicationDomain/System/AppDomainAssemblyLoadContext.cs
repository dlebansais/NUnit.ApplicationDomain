#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System;

using global::System.Reflection;
using global::System.Runtime.Loader;

/// <summary>
/// A <see cref="AssemblyLoadContext"/> that can resolve assembly loading.
/// </summary>
internal class AppDomainAssemblyLoadContext : AssemblyLoadContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDomainAssemblyLoadContext"/> class.
    /// </summary>
    /// <param name="name">The context name.</param>
    /// <param name="mainAssemblyToLoadPath">The path to the main assembly loaded in the context.</param>
    public AppDomainAssemblyLoadContext(string name, string mainAssemblyToLoadPath)
        : base(name, isCollectible: true)
    {
        Resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
    }

    /// <summary>
    /// Loads an assembly in nthe context.
    /// </summary>
    /// <param name="name">The assembly name.</param>
    protected override Assembly? Load(AssemblyName name)
    {
        string? assemblyPath = Resolver.ResolveAssemblyToPath(name);
        if (assemblyPath is not null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    private AssemblyDependencyResolver Resolver;
}

#endif

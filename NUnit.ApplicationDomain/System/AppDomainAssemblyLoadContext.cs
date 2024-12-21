#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System;

using global::System.Reflection;
using global::System.Runtime.Loader;

/// <summary>
/// A <see cref="AssemblyLoadContext"/> that can resolve assembly loading.
/// </summary>
/// <param name="name">The context name.</param>
/// <param name="mainAssemblyToLoadPath">The path to the main assembly loaded in the context.</param>
internal class AppDomainAssemblyLoadContext(string name, string mainAssemblyToLoadPath) : AssemblyLoadContext(name, isCollectible: true)
{
    /// <summary>
    /// Loads an assembly in nthe context.
    /// </summary>
    /// <param name="name">The assembly name.</param>
    protected override Assembly? Load(AssemblyName name)
    {
        return Resolver.ResolveAssemblyToPath(name) is string AssemblyPath
            ? LoadFromAssemblyPath(AssemblyPath)
            : null;
    }

    private readonly AssemblyDependencyResolver Resolver = new(mainAssemblyToLoadPath);
}

#endif

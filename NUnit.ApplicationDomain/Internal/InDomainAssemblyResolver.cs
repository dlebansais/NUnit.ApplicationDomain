namespace NUnit.ApplicationDomain.Internal;

using global::System;
using global::System.Collections.Generic;
using global::System.Diagnostics;
using global::System.Reflection;

/// <summary>
///  Resolves assemblies by delegating to the parent app domain for assembly locations.
/// </summary>
/// <param name="resolveHelper"> The resolve helper from the parent app domain. </param>
/// <remarks>Runs in the test app domain.</remarks>
[Serializable]
internal sealed class InDomainAssemblyResolver(ResolveHelper resolveHelper)
{
    // Creates an assembly resolver for all assemblies which might not be in the same path as the NUnit.ApplicationDomain assembly.
    // Although this object is created in the original app domain, it is serialized/copied into the test app domain and thus all methods except the constructor are invoked in the test domain.
    private readonly ResolveHelper ResolveHelper = resolveHelper;
    private readonly Dictionary<string, Assembly?> ResolvedAssemblies = [];

    /// <summary>
    /// Handles the <see cref="AppDomain.TypeResolve"/>, <see cref="AppDomain.ResourceResolve"/>, or <see cref="AppDomain.AssemblyResolve"/> event of an AppDomain.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The event data.</param>
    /// <returns>The assembly that resolves the type, assembly, or resource; or <see langword="null"/> if the assembly cannot be resolved.</returns>
    public Assembly? ResolveEventHandler(object? sender, ResolveEventArgs args)
    {
        if (ResolvedAssemblies.TryGetValue(args.Name, out Assembly? assembly))
        {
            return assembly;
        }

        // Not yet known => Store null in the dictionary (helps against stack overflow if a recursive call happens).
        ResolvedAssemblies[args.Name] = null;

        string? assemblyLocation = ResolveHelper.ResolveLocationOfAssembly(args.Name);
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            // The resolve helper found the assembly.
            assembly = Assembly.LoadFrom(assemblyLocation);
            ResolvedAssemblies[args.Name] = assembly;
            return assembly;
        }

        Debug.WriteLine($"Unknown assembly to be loaded: '{args.Name}', requested by '{args.RequestingAssembly}'");

        return null;
    }
}
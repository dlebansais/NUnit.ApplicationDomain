namespace NUnit.ApplicationDomain.Internal;

using global::System;
using global::System.IO;
using global::System.Reflection;

/// <summary> Helps to resolve the types in another app domain. </summary>
/// <remarks>
///  The methods are invoked and marshaled from the test app domain into the original domain.
/// </remarks>
[Serializable]
internal sealed class ResolveHelper : MarshalByRefObject
{
    /// <summary>
    /// Resolves the location of an assembly.
    /// </summary>
    /// <param name="assemblyName">The assembly name.</param>
    public static string? ResolveLocationOfAssembly(string assemblyName)
    {
        try
        {
            // Load the assembly. if loading fails, it can throw FileNotFoundException or
            // FileLoadException. Ignore those; this will return null.
            var assembly = Assembly.Load(assemblyName);
            return new Uri(assembly.Location).LocalPath;
        }
        catch (FileNotFoundException)
        {
        }
        catch (FileLoadException)
        {
        }

        return null;
    }
}
namespace NUnit.ApplicationDomain.System;

using global::System;
using global::System.Reflection;
using NUnit.ApplicationDomain.System.Security;
using NUnit.ApplicationDomain.System.Security.Policy;

public class AppDomain
{
    public static AppDomain CurrentDomain { get; } = new AppDomain();

    public object? CreateInstanceAndUnwrap(string assemblyName, string typeName)
    {
        throw new NotImplementedException();
    }

    public Assembly Load(AssemblyName assemblyRef)
    {
        throw new NotImplementedException();
    }

    public static AppDomain CreateDomain(string friendlyName, Evidence? securityInfo, AppDomainSetup info, PermissionSet grantSet, params StrongName[] fullTrustAssemblies)
    {
        throw new NotImplementedException();
    }

    public static void Unload(AppDomain domain)
    {
        throw new NotImplementedException();
    }

    public object GetData(string name)
    {
        throw new NotImplementedException();
    }

    public void SetData(string name, object data)
    {
        throw new NotImplementedException();
    }

    private void RaiseAssemblyResolve()
    {
        AssemblyResolve?.Invoke(this, new ResolveEventArgs(string.Empty));
    }

    public event ResolveEventHandler? AssemblyResolve;
}

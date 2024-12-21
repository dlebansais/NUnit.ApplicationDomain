namespace NUnit.ApplicationDomain;

using Contracts;
using global::System;
using AppDomain = System.AppDomain;

/// <summary>
///  Information about an app-domain constructed from an <see cref="IAppDomainFactory"/>.
/// </summary>
/// <param name="owner"> The factory that constructed this instance. </param>
/// <param name="appDomain"> The app domain to use for the test context. </param>
internal class ConstructedAppDomainInformation(IAppDomainFactory owner, AppDomain appDomain)
{
    /// <summary>
    /// Gets the factory that constructed this instance.
    /// </summary>
    public IAppDomainFactory Owner { get; } = Contract.AssertNotNull(owner);

    /// <summary>
    /// Gets the app domain to use for the app-domain context.
    /// </summary>
    public AppDomain AppDomain { get; } = Contract.AssertNotNull(appDomain);
}

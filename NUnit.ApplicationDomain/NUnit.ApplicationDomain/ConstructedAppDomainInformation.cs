namespace NUnit.ApplicationDomain;

using Contracts;
using global::System;
using AppDomain = System.AppDomain;

/// <summary>
///  Information about an app-domain constructed from an <see cref="IAppDomainFactory"/>.
/// </summary>
internal class ConstructedAppDomainInformation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructedAppDomainInformation"/> class.
    /// </summary>
    /// <param name="owner"> The factory that constructed this instance. </param>
    /// <param name="appDomain"> The app domain to use for the test context. </param>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    public ConstructedAppDomainInformation(IAppDomainFactory owner, AppDomain appDomain)
    {
        Owner = Contract.AssertNotNull(owner);
        AppDomain = Contract.AssertNotNull(appDomain);
    }

    /// <summary>
    /// Gets the factory that constructed this instance.
    /// </summary>
    public IAppDomainFactory Owner { get; }

    /// <summary>
    /// Gets the app domain to use for the app-domain context.
    /// </summary>
    public AppDomain AppDomain { get; }
}

namespace NUnit.ApplicationDomain;

using Contracts;
using global::System;
using global::System.IO;
using NUnit.ApplicationDomain.Internal;
using NUnit.Framework;
using AppDomain = System.AppDomain;
using AppDomainSetup = System.AppDomainSetup;
using PermissionSet = System.Security.PermissionSet;
using PermissionState = System.Security.Permissions.PermissionState;

/// <summary>
///  A default app-domain factory that simply constructs a new app-domain before the test and
///  tears it down after every test.
/// </summary>
internal partial class PerTestAppDomainFactory : IAppDomainFactory
{
    /// <inheritdoc />
    [Access("public", "virtual")]
    [RequireNotNull(nameof(testMethodInfo))]
    private ConstructedAppDomainInformation GetAppDomainForVerified(TestMethodInformation testMethodInfo)
    {
        var appDomainInfo = new AppDomainSetup
                            {
                            // At a minimum, we want the ability to load the types defined in this assembly
                            ApplicationBase = GetDirectoryToMyDll()
                            };

        if (!String.IsNullOrEmpty(testMethodInfo.AppConfigFile))
        {
            appDomainInfo.ConfigurationFile = testMethodInfo.AppConfigFile;
        }

        ConfigureAppDomain(appDomainInfo);

        var appDomain = ConstructAppDomain(testMethodInfo, appDomainInfo);

        return new ConstructedAppDomainInformation(this, appDomain);
    }

    /// <summary> Constructs the app domain to be used for the given test method. </summary>
    /// <param name="testMethodInfo"> Information describing the test method. </param>
    /// <param name="appDomainInfo"> The AppDomainSetup that was construct for the app domain. </param>
    /// <returns> The app domain to use for the given test. </returns>
    protected virtual AppDomain ConstructAppDomain(TestMethodInformation testMethodInfo, AppDomainSetup appDomainInfo)
    {
        return AppDomain.CreateDomain(AppDomainRunner.TestAppDomainName,
                                    null,
                                    appDomainInfo,
                                    GetPermissionSet(testMethodInfo));
    }

    /// <inheritdoc />
    [Access("public", "virtual")]
    [RequireNotNull(nameof(constructedInfo))]
    private static void MarkFinishedVerified(ConstructedAppDomainInformation constructedInfo)
    {
        // if we don't unload, it's possible that execution continues in the AppDomain, consuming CPU/
        // memory.  See more info @ https://bitbucket.org/zastrowm/nunit.applicationdomain/pull-requests/1/ 
        AppDomain.Unload(constructedInfo.AppDomain);
    }

    /// <summary> Method that allows sub-classes to configure how the app-domain is setup. </summary>
    /// <param name="appDomainSetup"> The AppDomainSetup that will be used to construct the instance. </param>
    protected virtual void ConfigureAppDomain(AppDomainSetup appDomainSetup)
    {
    }

    /// <summary> Gets the permission set for the constructed app-domains. </summary>
    /// <returns> The permission set to use for constructed app-domains. </returns>
    protected virtual PermissionSet GetPermissionSet(TestMethodInformation context)
    {
        return new PermissionSet(PermissionState.Unrestricted);
    }

    /// <summary> Gets the directory where the dlls for NUnit.Application.Domain can be found. </summary>
    private static string? GetDirectoryToMyDll()
    {
        return Path.GetDirectoryName(new Uri(typeof(InDomainTestMethodRunner).Assembly.Location).LocalPath);
    }
}

namespace NUnit.ApplicationDomain.Tests;

using global::System;
using NUnit.Framework;
#if NET8_0_OR_GREATER
using AppDomain = System.AppDomain;
using AppDomainSetup = System.AppDomainSetup;
#endif

public class FactoryTests
{
    [Test, RunInApplicationDomain(AppDomainFactory = typeof(AppDomainFactoryImplementation))]
    public void VerifyFactoryIsInvoked()
    {
        var data = AppDomain.CurrentDomain.GetData("Example-Data") as bool?;
        Assert.That(data, Is.True);
    }

    public class AppDomainFactoryImplementation : PerTestAppDomainFactory
    {
        protected override AppDomain ConstructAppDomain(TestMethodInformation testMethodInfo, AppDomainSetup appDomainInfo)
        {
            var appDomain = base.ConstructAppDomain(testMethodInfo, appDomainInfo);
            appDomain.SetData("Example-Data", true);
            return appDomain;
        }
    }
}

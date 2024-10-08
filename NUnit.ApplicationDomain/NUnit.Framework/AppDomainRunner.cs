﻿namespace NUnit.Framework;

using System;
using NUnit.Framework.Internal;

/// <summary>
/// Helps to run a test in another application domain.
/// </summary>
public static class AppDomainRunner
{
    /// <summary> The name of the app-domain in which tests are run. </summary>
    public const string TestAppDomainName
      = "NUnit.ApplicationDomain ({9421D297-D477-4CEE-9C09-38BCC1AB5176})";

    private const string PropertyBagKeyForSharedProperties
      = "{C50C4E3C-4A27-4542-848D-2DCDED92DF77}";

    /// <summary>
    /// Initializes static members of the <see cref="AppDomainRunner"/> class.
    /// </summary>
    static AppDomainRunner()
    {
        ShouldIncludeAppDomainErrorMessages = true;
    }

    /// <summary>
    /// Gets a value indicating whether the current test is being executed in an application domain created by the <see cref="RunInApplicationDomainAttribute"/>.
    /// </summary>
    public static bool IsInTestAppDomain { get; internal set; }

    /// <summary>
    ///  Gets a value indicating whether the current test is not being executed in an application domain created by the <see cref="RunInApplicationDomainAttribute"/>.
    /// </summary>
    /// <remarks> Equivalent to !IsInTestAppDomain. </remarks>
    public static bool IsNotInTestAppDomain
      => !IsInTestAppDomain;

    /// <summary>
    ///  Gets or sets a value indicating whether messages should be printed to standard output when a test failure occurs while in the test app domain.
    /// </summary>
    /// <remarks> True by default. </remarks>
    public static bool ShouldIncludeAppDomainErrorMessages { get; set; }

    /// <summary>
    ///  Gets properties that are carried into the app-domain and are carried out when the test is over.
    /// </summary>
    public static SharedDataStore? DataStore
    {
        get
        {
            if (IsNotInTestAppDomain)
            {
                // in the parent domain, we store it in the current test info.
                SharedDataStore? properties;

                var propertyBag = TestExecutionContext.CurrentContext.CurrentTest.Properties;
                if (propertyBag.ContainsKey(PropertyBagKeyForSharedProperties))
                {
                    properties = (SharedDataStore?)propertyBag.Get(PropertyBagKeyForSharedProperties);
                }
                else
                {
                    properties = new SharedDataStore();
                    propertyBag.Set(PropertyBagKeyForSharedProperties, properties);
                }

                return properties;
            }
            else
            {
                if (HiddenDataStore is not null)
                    return HiddenDataStore;

                throw new InvalidOperationException(
                        $"For some reason, the {typeof(SharedDataStore)} was not flowed into the test-domain");
            }
        }
    }

    /// <summary>
    /// Gets or sets the fake data-store set before the test runs.
    /// </summary>
    internal static SharedDataStore? HiddenDataStore { get; set; }
}

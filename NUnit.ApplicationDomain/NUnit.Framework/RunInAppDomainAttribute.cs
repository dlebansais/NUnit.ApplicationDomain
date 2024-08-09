namespace NUnit.Framework;

using global::System;
using NUnit.ApplicationDomain;
using NUnit.ApplicationDomain.Internal;
using NUnit.Framework.Interfaces;
#if NET8_0_OR_GREATER
using AppDomain = System.AppDomain;
#endif

/// <summary> Indicates that a test should be run in a separate application domain. </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RunInApplicationDomainAttribute : TestActionAttribute
{
    /// <summary>
    ///  The app-domain factory to use when constructing app domains.  Must be an instance of
    ///  <see cref="IAppDomainFactory"/>
    /// </summary>
    public Type? AppDomainFactory { get; set; }

    /// <inheritdoc />
    public override void BeforeTest(ITest test)
    {
        // only continue execution if
        if (AppDomain.CurrentDomain.FriendlyName == AppDomainRunner.TestAppDomainName)
            return;

        RunInApplicationDomain(test);
    }

    /// <summary>
    ///  Check if we're in the "test" appdomain, and if we aren't, run the given test in an appdomain,
    ///  capture the result, and propagate it back.
    /// </summary>
    private void RunInApplicationDomain(ITest testDetails)
    {
        var exception = ParentAppDomainRunner.Run(testDetails, AppDomainFactory);

        if (exception == null)
        {
            Assert.Pass();
        }

        if (AppDomainRunner.ShouldIncludeAppDomainErrorMessages)
        {
            // Compare names because the exception is coming from a different AppDomain.
            if (exception.GetType().FullName == typeof(SuccessException).FullName)
            {
                // Don't output anything in case of success.
                // Just throw a copy of the original exception.
                throw new SuccessException(exception.Message);
            }
            else if (exception is AssertionException)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("======================================");
                Console.Error.WriteLine("Assertion failed in Application Domain");
                Console.Error.WriteLine("======================================");
            }
            else
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("======================================");
                Console.Error.WriteLine("Exception thrown in application domain");
                Console.Error.WriteLine("======================================");
            }
        }

        throw exception;
    }

    /// <inheritdoc />
    public override ActionTargets Targets
        => ActionTargets.Test;
}

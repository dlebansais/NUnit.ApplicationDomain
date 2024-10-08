﻿namespace NUnit.ApplicationDomain.Internal;

using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

/// <summary>
///  Facilitates retrieving test execution context arguments.
/// </summary>
internal static class CurrentArgumentsRetriever
{
    /// <summary>
    ///  Attempts to get the arguments for the current executing test.  Used to use reflection but was exposed publicly in NUnit 3.7.
    /// </summary>
    /// <param name="test">The tests that were passed into method for the given test.</param>
    /// <returns>The current arguments for the test, or null of none are available.</returns>
    public static object?[] GetTestArguments(ITest test)
      => test.Arguments;

    /// <summary>
    ///  Gets the test-fixture arguments that were used to construct the test fixture associated with the given test.
    /// </summary>
    /// <param name="test">The tests that were passed into the test fixture for the given test.</param>
    /// <returns>The arguments used to construct the test fixture, or null if no arguments were used.</returns>
    public static object?[]? GetTestFixtureArguments(ITest test)
    {
        return FindFixture(test)?.Arguments;
    }

    /// <summary>
    /// Finds the test fixture associated with the given test.
    /// </summary>
    /// <param name="test">The tests that were passed into method for the given test.</param>
    /// <returns>The test fixture associated with the given test.</returns>
    private static TestFixture? FindFixture(ITest? test)
    {
        if (test is null)
            return null;

        var fixture = test as TestFixture;
        if (fixture is not null)
            return fixture;

        return FindFixture(test.Parent);
    }
}

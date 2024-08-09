namespace NUnit.ApplicationDomain.Internal;

using Contracts;
using global::System;
using global::System.Collections.Generic;
using global::System.Reflection;

/// <summary> The setup and teardown methods to invoke before running a test. </summary>
internal sealed class SetupAndTeardownMethods : MarshalByRefObject
{
    /// <summary> Constructor. </summary>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    /// <param name="setupMethods"> The setup methods for the current test. </param>
    /// <param name="teardownMethods"> The teardown methods for the current test. </param>
    public SetupAndTeardownMethods(IEnumerable<MethodBase> setupMethods, IEnumerable<MethodBase> teardownMethods)
    {
        SetupMethods = Contract.AssertNotNull(setupMethods);
        TeardownMethods = Contract.AssertNotNull(teardownMethods);
    }

    /// <summary> The setup methods for the current test. </summary>
    public IEnumerable<MethodBase> SetupMethods { get; }

    /// <summary> The teardown methods for the current test. </summary>
    public IEnumerable<MethodBase> TeardownMethods { get; }
}
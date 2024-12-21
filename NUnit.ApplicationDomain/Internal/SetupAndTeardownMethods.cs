namespace NUnit.ApplicationDomain.Internal;

using Contracts;
using global::System;
using global::System.Collections.Generic;
using global::System.Reflection;

/// <summary> The setup and teardown methods to invoke before running a test. </summary>
/// <param name="setupMethods"> The setup methods for the current test. </param>
/// <param name="teardownMethods"> The teardown methods for the current test. </param>
internal sealed class SetupAndTeardownMethods(IEnumerable<MethodBase> setupMethods, IEnumerable<MethodBase> teardownMethods) : MarshalByRefObject
{
    /// <summary>
    /// Gets the setup methods for the current test.
    /// </summary>
    public IEnumerable<MethodBase> SetupMethods { get; } = Contract.AssertNotNull(setupMethods);

    /// <summary>
    /// Gets the teardown methods for the current test.
    /// </summary>
    public IEnumerable<MethodBase> TeardownMethods { get; } = Contract.AssertNotNull(teardownMethods);
}

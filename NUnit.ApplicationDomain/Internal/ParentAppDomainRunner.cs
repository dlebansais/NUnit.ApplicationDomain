namespace NUnit.ApplicationDomain.Internal;

using Contracts;
using global::System;
using global::System.Collections.Concurrent;
using global::System.Collections.Generic;
using global::System.Reflection;
using global::System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using AppDomain = System.AppDomain;
using PermissionSet = System.Security.PermissionSet;
using PermissionState = System.Security.Permissions.PermissionState;

/// <summary> Runs a TestMethodInformation in a child app domain. </summary>
internal static partial class ParentAppDomainRunner
{
    /// <summary> The setup/teardown methods that have been cached for each type thus far. </summary>
    private static readonly ConcurrentDictionary<Type, SetupAndTeardownMethods> CachedInfo = new();

    private static readonly PerTestAppDomainFactory DefaultFactory = new();

    /// <summary> Runs the given test for the given type under a new, clean app domain. </summary>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
    ///  illegal values. </exception>
    /// <param name="test"> The test that should be run in another app-domain. </param>
    /// <param name="appDomainFactoryType"> The type of factory to use to construct app-domains. </param>
    /// <returns>
    ///  The exception that occurred while executing the test, or null if no exception was generated.
    /// </returns>
    [RequireNotNull(nameof(test))]
    private static Exception? RunVerified(ITest test, Type? appDomainFactoryType)
    {
        IAppDomainFactory appDomainFactory = ConstructFactory(appDomainFactoryType);
        ITypeInfo? typeInfo = GetTypeInfo(test) ?? throw new ArgumentException("Cannot determine the type that the test belongs to");
        SetupAndTeardownMethods setupAndTeardown = GetSetupTeardownMethods(typeInfo.Type);

        object?[] testArguments = CurrentArgumentsRetriever.GetTestArguments(test);
        object?[]? testFixtureArguments = CurrentArgumentsRetriever.GetTestFixtureArguments(test);
        MethodInfo? testMethod = test.Method?.MethodInfo;

        TestMethodInformation methodData = new(typeInfo.Type,
                                               testMethod,
                                               setupAndTeardown,
                                               AppDomainRunner.DataStore,
                                               testArguments,
                                               testFixtureArguments);

        Exception? possibleException = RunInternal(appDomainFactory, methodData, out WeakReference weakRef);

        for (int i = 0; weakRef.IsAlive && i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        return possibleException;
    }

    private static ITypeInfo? GetTypeInfo(ITest test)
    {
        return test.Fixture is not null
          ? test.TypeInfo
          : test.Method?.TypeInfo;
    }

    // No inlining to make sure no spurious reference remain behind upon return (for garbage collection).
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Exception? RunInternal(IAppDomainFactory appDomainFactory, TestMethodInformation methodData, out WeakReference weakRef)
    {
        ConstructedAppDomainInformation domainInfo = appDomainFactory.GetAppDomainFor(methodData);
        AppDomain domain = domainInfo.AppDomain;

        // Add an assembly resolver for resolving any assemblies not known by the test application domain.
        InDomainAssemblyResolver assemblyResolver = new(new ResolveHelper());
        domain.AssemblyResolve += assemblyResolver.ResolveEventHandler;

        weakRef = new WeakReference(domain, trackResurrection: true);

#if NET8_0_OR_GREATER
        domain.Load(methodData.TypeUnderTest.Assembly.Location);

        object? inDomainRunner = domain.CreateInstanceAndUnwrap<InDomainTestMethodRunner>(usePublicConstructor: true);

        if (!domain.TryClone(methodData, out object? CloneMethodData))
            throw new InvalidOperationException();

        MethodInfo? executeMethod = inDomainRunner?.GetType().GetMethod("Execute");

        // Store any resulting exception from executing the test method
        Exception? possibleException = executeMethod?.Invoke(inDomainRunner, [CloneMethodData]) as Exception;

        if (methodData.DataStore is object DataStore)
        {
            FieldInfo StoreLookupField = Contract.AssertNotNull(DataStore.GetType().GetField("lookup", BindingFlags.Instance | BindingFlags.NonPublic));
            Dictionary<string, object?> Lookup = (Dictionary<string, object?>)Contract.AssertNotNull(StoreLookupField.GetValue(DataStore));

            PropertyInfo DataStoreProperty = Contract.AssertNotNull(Contract.AssertNotNull(CloneMethodData).GetType().GetProperty("DataStore"));
            object ClonedDataStore = Contract.AssertNotNull(DataStoreProperty.GetValue(CloneMethodData));
            FieldInfo ClonedStoreLookupField = Contract.AssertNotNull(ClonedDataStore.GetType().GetField("lookup", BindingFlags.Instance | BindingFlags.NonPublic));
            Dictionary<string, object?>? ClonedLookup = (Dictionary<string, object?>)Contract.AssertNotNull(ClonedStoreLookupField.GetValue(ClonedDataStore));

            Lookup.Clear();
            foreach (string Key in ClonedLookup.Keys)
                Lookup.Add(Key, ClonedLookup[Key]);
        }
#else
        domain.Load(methodData.TypeUnderTest.Assembly.GetName());

        InDomainTestMethodRunner? inDomainRunner = domain.CreateInstanceAndUnwrap<InDomainTestMethodRunner>();

        // Store any resulting exception from executing the test method
        inDomainRunner?.Execute(methodData);
        Exception? possibleException = inDomainRunner?.LastExceptionCaught;
#endif

        domainInfo.Owner.MarkFinished(domainInfo);

        return possibleException;
    }

    /// <summary>
    ///  Construct the <see cref="IAppDomainFactory"/> from the given type, throwing out if the type
    ///  is not an instance of
    ///  <see cref="IAppDomainFactory"/>.
    /// </summary>
    private static IAppDomainFactory ConstructFactory(Type? typeToConstruct)
    {
        if (typeToConstruct is null)
            return DefaultFactory;

        object? instance = Activator.CreateInstance(typeToConstruct);
        IAppDomainFactory? factory = instance as IAppDomainFactory;

        return factory ?? throw new InvalidOperationException($"Cannot specify an AppDomainFactory that is not an instance of ${nameof(IAppDomainFactory)}");
    }

    /// <summary> Gets the setup and teardown methods for the given type. </summary>
    /// <param name="typeUnderTest"> The type under test. </param>
    /// <returns>
    ///  The setup teardown methods, loaded from the cache if it already existed, otherwise queried
    ///  via reflection.
    /// </returns>
    private static SetupAndTeardownMethods GetSetupTeardownMethods(Type typeUnderTest)
    {
        if (CachedInfo.TryGetValue(typeUnderTest, out SetupAndTeardownMethods? setupAndTeardown))
            return setupAndTeardown;

        // get all of the setup methods in the type
        List<MethodInfo> setupMethods = typeUnderTest.GetMethodsWithAttribute<OneTimeSetUpAttribute>();
        setupMethods.AddRange(typeUnderTest.GetMethodsWithAttribute<SetUpAttribute>());

        // we want most-derived last
        setupMethods.Reverse();

        // get all of the teardown methods in the type (it is already the way we want it).
        List<MethodInfo> teardownMethods = typeUnderTest.GetMethodsWithAttribute<OneTimeTearDownAttribute>();
        teardownMethods.AddRange(typeUnderTest.GetMethodsWithAttribute<TearDownAttribute>());

        setupAndTeardown = new SetupAndTeardownMethods(setupMethods, teardownMethods);
        _ = CachedInfo.TryAdd(typeUnderTest, setupAndTeardown);

        return setupAndTeardown;
    }

    /// <summary>
    /// create a permission set.
    /// </summary>
    private static PermissionSet GetPermissionSet()
        => new(PermissionState.Unrestricted);
}

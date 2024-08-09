namespace NUnit.ApplicationDomain;

using Contracts;
using global::System;
using global::System.Collections.Generic;
using global::System.IO;
using global::System.Reflection;
using NUnit.ApplicationDomain.Internal;
using NUnit.Framework;

/// <summary> All of the arguments for the TestExecutor. </summary>
internal class TestMethodInformation : MarshalByRefObject
{
    /// <summary> Constructor. </summary>
    /// <exception cref="ArgumentNullException"> When one or more required arguments are null. </exception>
    /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
    ///  illegal values. </exception>
    /// <param name="typeUnderTest"> The type that the method belongs to and which will be
    ///  instantiated in the test app domain. </param>
    /// <param name="methodUnderTest"> The method to invoke as the core unit of the test. </param>
    /// <param name="methods"> The setup and teardown methods to invoke before/after running the test. </param>
    /// <param name="dataStore"> The data store to install into the test AppDomain. </param>
    /// <param name="arguments"> The arguments to pass into the test method. </param>
    /// <param name="fixtureArguments"> The arguments to use when constructing the test fixture. </param>
    internal TestMethodInformation(Type? typeUnderTest,
                                   MethodBase? methodUnderTest,
                                   SetupAndTeardownMethods? methods,
                                   SharedDataStore? dataStore,
                                   IReadOnlyList<object?>? arguments,
                                   IReadOnlyList<object?>? fixtureArguments)
    {
        TypeUnderTest = Contract.AssertNotNull(typeUnderTest);
        MethodUnderTest = Contract.AssertNotNull(methodUnderTest);
        Methods = Contract.AssertNotNull(methods);
        DataStore = dataStore;

        Contract.AssertNotNull(MethodUnderTest.DeclaringType);

        string? configFile = FindConfigFile(Assembly.GetAssembly(TypeUnderTest)!);
        AppConfigFile = configFile;

        OutputStream = Console.Out;
        ErrorStream = Console.Error;

        Arguments = arguments;
        FixtureArguments = fixtureArguments;
    }

    /// <summary> The setup and teardown methods to invoke before/after running the test. </summary>
    internal SetupAndTeardownMethods Methods { get; }

    /// <summary> System.Out. </summary>
    internal TextWriter OutputStream { get; }

    /// <summary> System.Err. </summary>
    internal TextWriter ErrorStream { get; }

    /// <summary>
    ///  The that contains the method to run in the application domain.
    /// </summary>
    public Type TypeUnderTest { get; }

    /// <summary> Gets or sets the name of the test. </summary>
    public MethodBase MethodUnderTest { get; }

    /// <summary>
    ///  Any additional parameters to give to the test method, normally set via TestCaseAttribute.
    /// </summary>
    public IReadOnlyList<object?>? Arguments { get; }

    /// <summary>
    ///  Parameters to give to the test constructor, normally set via TestFixtureAttribute.
    /// </summary>
    public IReadOnlyList<object?>? FixtureArguments { get; }

    /// <summary>  The data store to install into the test AppDomain. </summary>
    public SharedDataStore? DataStore { get; set; }

    /// <summary> The app config file for the test. </summary>
    public string? AppConfigFile { get; }

    /// <summary> Try to get the AppConfig file for the assembly. </summary>
    /// <param name="assembly"> The assembly whose app config file should be retrieved. </param>
    /// <returns> The path to the config file, or null if it does not exist. </returns>
    private static string? FindConfigFile(Assembly assembly)
    {
        string configFile = new Uri(assembly.Location).LocalPath + ".config";
        return File.Exists(configFile) ? configFile : null;
    }
}
﻿namespace NUnit.ApplicationDomain.Internal;

using global::System;
using global::System.Linq;
using global::System.Reflection;
using global::System.Threading.Tasks;
using NUnit.Framework;

/// <summary> Executes a test method in the application domain. </summary>
/// <returns> Runs in the test app domain. </returns>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes: instanciated with a call to CreateInstanceAndUnwrap.
internal sealed class InDomainTestMethodRunner : MarshalByRefObject
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
{
    /// <summary>
    /// Gets the last exception that occurred as a result of executing <see cref="Execute(TestMethodInformation)"/>.
    /// </summary>
    public Exception? LastExceptionCaught { get; private set; }

    /// <summary> Executes the test method indicates by <paramref name="testMethodInfo"/>. </summary>
    /// <param name="testMethodInfo"> Information that describes the test method to execute. </param>
    public void Execute(TestMethodInformation testMethodInfo)
    {
        AppDomainRunner.HiddenDataStore = testMethodInfo.DataStore;

        // Forward this data from the outside application domain
        Console.SetOut(testMethodInfo.OutputStream);
        Console.SetError(testMethodInfo.ErrorStream);

        Type typeUnderTest = testMethodInfo.TypeUnderTest;

        // mark this test as being in the app domain.  As soon as we're done, we're going to tear down
        // the app domain, so there is no need to set this back to false.
        AppDomainRunner.IsInTestAppDomain = true;

        object? instance;
        if (testMethodInfo.FixtureArguments is null)
        {
            instance = Activator.CreateInstance(typeUnderTest);
        }
        else
        {
            instance = Activator.CreateInstance(typeUnderTest, testMethodInfo.FixtureArguments.ToArray());
        }

        // run setup and test, with an exception handler
        LastExceptionCaught = RunSetupAndTest(testMethodInfo, instance);
        var teardownException = RunTeardown(testMethodInfo, instance);

        LastExceptionCaught ??= teardownException;
    }

    /// <summary> Runs the setup and test method, returning the exception that occurred or null if no exception was fired. </summary>
    private static Exception? RunSetupAndTest(TestMethodInformation testMethodInfo, object? instance)
    {
        try
        {
            foreach (var setupMethod in testMethodInfo.Methods.SetupMethods)
            {
                setupMethod.Invoke(instance, null);
            }

            var taskResult = testMethodInfo.MethodUnderTest.Invoke(instance, testMethodInfo.Arguments?.ToArray()) as Task;
            if (taskResult is not null)
            {
                var handler = CreateAsyncTestResultHandler(instance);
                handler.Process(taskResult);
            }
        }
        catch (TargetInvocationException e)
        {
            return e.InnerException;
        }

        return null;
    }

    /// <summary>
    ///  Creates the <see cref="IAsyncTestResultHandler"/> to handle an
    ///  async test result.
    /// </summary>
    private static IAsyncTestResultHandler CreateAsyncTestResultHandler(object? instance)
    {
        return instance as IAsyncTestResultHandler
               ?? new TaskWaitTestResultHandler();
    }

    /// <summary> Run each teardown method, returning the first exception that occurred, if any. </summary>
    private static Exception? RunTeardown(TestMethodInformation testMethodInfo, object? instance)
    {
        Exception? exception = null;

        foreach (var teardownMethod in testMethodInfo.Methods.TeardownMethods)
        {
            try
            {
                teardownMethod.Invoke(instance, null);
            }
            catch (TargetInvocationException e)
            {
                // we only save the first exception
                if (exception is null)
                {
                    exception = e.InnerException;
                }
            }
        }

        return exception;
    }
}
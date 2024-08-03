namespace NUnit.ApplicationDomain.Tests;

using global::System.Threading;
using global::System;
using global::System.Threading.Tasks;
using NUnit.Framework;
using global::System.Windows.Threading;

[RunInApplicationDomain]
internal class AsyncTestWithDispatcherRunner : IAsyncTestResultHandler
{
    [SetUp]
    public void Setup()
    {
      if (AppDomainRunner.IsNotInTestAppDomain)
        return;

    // install a sync context so that awaits continue on the dispatcher.
#if NET8_0_OR_GREATER
    SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
#else
    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
#endif
    }

    [Test]
    public async Task WaitsForDispatcherToContinue()
    {
        // pretend that for some made-up reason, we need to be in the event loop 
#if NET8_0_OR_GREATER
        await Dispatcher.Yield();
#else
        await Dispatcher.Yield();
#endif

        // and pretend that something later triggers that allows us to complete
        await Task.Delay(TimeSpan.FromSeconds(3));

        AppDomainRunner.DataStore.Set<bool>("ran", true);
    }

    [TearDown]
    public void Teardown()
    {
        Assert.That(AppDomainRunner.DataStore.Get<bool>("ran"), Is.True);
    }

    /// <inheritdoc />
    void IAsyncTestResultHandler.Process(Task task)
    {
        // if we just simply did task.Wait(), we would block indefinitely because no-one is message
        // pumping. 

        // instead, tell the dispatcher to run until the task has resolved
        var frame = new DispatcherFrame();
        task.ContinueWith(_1 => frame.Continue = false);
        Dispatcher.PushFrame(frame);

        // propagate any exceptions
        task.Wait();
    }
}
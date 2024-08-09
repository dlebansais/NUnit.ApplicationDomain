namespace NUnit.ApplicationDomain;

using global::System;
using global::System.Threading.Tasks;

/// <summary>
///  A <see cref="IAsyncTestResultHandler"/> that simply invokes <see cref="Task.Wait()"/> on the
///  result of a task-returning test.
/// </summary>
public class TaskWaitTestResultHandler : IAsyncTestResultHandler
{
    /// <inheritdoc />
    public void Process(Task task)
    {
        if (task is null)
            throw new ArgumentNullException(nameof(task));
        task.Wait();
    }
}

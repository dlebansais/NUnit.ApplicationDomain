namespace NUnit.ApplicationDomain;

using Contracts;
using global::System.Threading.Tasks;

/// <summary>
///  A <see cref="IAsyncTestResultHandler"/> that simply invokes <see cref="Task.Wait()"/> on the result of a task-returning test.
/// </summary>
internal partial class TaskWaitTestResultHandler : IAsyncTestResultHandler
{
    /// <inheritdoc cref="IAsyncTestResultHandler.Process(Task)" />
    [Access("public")]
    [RequireNotNull(nameof(task))]
    private static void ProcessVerified(Task task)
    {
        task.Wait();
    }
}

namespace NUnit.ApplicationDomain;

using global::System.Threading.Tasks;

/// <summary>
///  Api that allows the task result of async methods to be handled via whatever means a framework
///  would like.
/// </summary>
internal interface IAsyncTestResultHandler
{
    /// <summary>
    ///  Invoked when a test that returns a task has been executed. Should not return until the "task"
    ///  is considered complete.
    /// </summary>
    /// <param name="task"> The task that the test method returned. </param>
    void Process(Task task);
}
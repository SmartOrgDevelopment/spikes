using System.Collections.Generic;

namespace AsyncWorkQueueTest
{
    public delegate T AsyncWorker<T>();

    /// <summary>
    /// Work manager that allows a number of items to be executed concurrently and collected synchronously.
    /// </summary>
    /// <typeparam name="T">The type of result the work produces.</typeparam>
    public interface IAsyncWorkQueue<T>
    {
        /// <summary>
        /// Enqueues the specified work to be performed.
        /// </summary>
        /// <param name='work'>
        /// A delegate that performs long-running, parallelizable work and returns a result.
        /// </param>
        void Start(AsyncWorker<T> work);

        /// <summary>
        /// Blocks until all queued work is completed and then returns the results.
        /// </summary>
        /// <returns>
        /// The results of the work that was queued.
        /// </returns>
        IEnumerable<AsyncWorkResult<T>> GetResults();
    }
}
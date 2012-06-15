using System;
using System.Collections.Generic;
using System.Threading;

#pragma warning disable 0420

namespace AsyncWorkQueueTest
{
    /// <summary>
    /// Work manager that allows a number of items to be executed concurrently and collected synchronously.
    /// </summary>
    /// <typeparam name="T">The type of result the work produces.</typeparam>
    /// <remarks>
    /// User code should call <see cref="Start"/> for each atomic, parallelizable task to be performed
    /// and then call <see cref="GetResults"/> to wait for all tasks to complete.
    /// </remarks>
    /// <example>
    /// <![CDATA[
    /// using (AsyncWorkQueue<int> queue = new AsyncWorkQueue<int>(5)) 
    /// {
    /// 	for (int i = 0; i < 10; i++) 
    /// 	{
    /// 		int time = i * 100;
    /// 		int result = Math.Pow(i, 2);
    /// 		queue.Start(delegate 
    /// 		{
    /// 			Thread.Sleep(time); // work
    /// 			return result;
    /// 		});
    /// 	}
    /// 
    /// 	foreach (var result in queue.GetResults()) // this will block until everything is done.
    /// 	{
    /// 		if (result.IsError)
    /// 			throw result.Error; // any error that occurs executing the task will be stored here.
    /// 
    /// 		Console.WriteLine(result.Value);
    /// 	}
    /// } // disposing the queue ensures all worker threads are terminated.
    /// ]]>
    /// </example>
    public class AsyncWorkQueue<T> : IAsyncWorkQueue<T>, IDisposable
    {
        private readonly List<Thread> _threads;
        private readonly int _threadCount;
        private readonly Queue<AsyncWorker<T>> _workQueue = new Queue<AsyncWorker<T>>();
        private readonly List<AsyncWorkResult<T>> _results = new List<AsyncWorkResult<T>>();
        private readonly Semaphore _workSync = new Semaphore(0, int.MaxValue);
        private readonly ManualResetEvent _getResultsEvent = new ManualResetEvent(true);
        private volatile int _runningThreads;
        private volatile int _isDisposed; // 0 => false, nonzero => true

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncWorkQueueTest.AsyncWorkQueue`1"/> class.
        /// </summary>
        /// <param name='threadCount'>The maximum number of threads to create.</param>
        public AsyncWorkQueue(int threadCount)
        {
            if (threadCount < 1) throw new ArgumentOutOfRangeException("threadCount", "must be greater than or equal to 1.");

            _threadCount = threadCount;
            _threads = new List<Thread>(threadCount);
        }

        private bool IsDisposed { get { return _isDisposed != 0; } }

        /// <summary>
        /// Enqueues the specified work to be performed.
        /// </summary>
        /// <param name='work'>
        /// A delegate that performs long-running, parallelizable work and returns a result.
        /// </param>
        /// <exception cref='ObjectDisposedException'>
        /// Is thrown when this operation is performed after <see cref="Dispose"/> has been called.
        /// </exception>
        public void Start(AsyncWorker<T> work)
        {
            if (work == null) throw new ArgumentNullException("work");

            if (IsDisposed) throw new ObjectDisposedException(null);

            // Now that we have work, make sure we block GetResults()
            _getResultsEvent.Reset();

            // Lazily make threads
            if (_threads.Count < _threadCount)
                lock (_threads)
                    if (_threads.Count < _threadCount)
                    {
                        Thread thread = new Thread(ProcessQueue);

                        thread.Name = string.Format("AsyncWorkQueue thread {0}", _threads.Count + 1);
                        thread.IsBackground = true;

                        thread.Start();
                        _threads.Add(thread);
                    }

            lock (_workQueue)
                _workQueue.Enqueue(work);

            _workSync.Release();
        }

        /// <summary>
        /// Blocks until all queued work is completed and then returns the results.
        /// </summary>
        /// <returns>
        /// The results of the work that was queued.
        /// </returns>
        /// <remarks>
        /// Calls to this method are serialized.
        /// </remarks>
        /// <exception cref='ObjectDisposedException'>
        /// Is thrown when this operation is performed after <see cref="Dispose"/> has been called.
        /// </exception>
        public IEnumerable<AsyncWorkResult<T>> GetResults()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            // lock on the event itself to serialize calls to GetResults().
            lock (_getResultsEvent)
            {
                // Wait for the signal from the work threads.
                _getResultsEvent.WaitOne();

                // If new work is enqueue between the WaitOne() and here, make sure
                // it doesn't try to add to our results while we're copying them out.
                lock (_results)
                {
                    // return a copy so no one mutates our internal state.
                    AsyncWorkResult<T>[] copy = new AsyncWorkResult<T>[_results.Count];
                    _results.CopyTo(copy);
                    _results.Clear();

                    return copy;
                }
            }
        }

        private void ProcessQueue()
        {
            while (!IsDisposed)
            {
                _workSync.WaitOne();

                // Did we wake up because we're being disposed?
                if (IsDisposed)
                    break;

                // Track that we're doing work now.
                Interlocked.Increment(ref _runningThreads);

                try
                {
                    AsyncWorker<T> work;
                    lock (_workQueue)
                        work = _workQueue.Dequeue();

                    AsyncWorkResult<T> result;
                    try
                    {
                        result = work();
                    }
                    catch (Exception error)
                    {
                        result = new AsyncWorkResult<T>(error);
                    }

                    lock (_results)
                        _results.Add(result);
                }
                finally
                {
                    // Done doing work now
                    Interlocked.Decrement(ref _runningThreads);
                }

                // is no one working now?
                if (_runningThreads == 0)
                    lock (_workQueue)
                        // is there any work left? (double check no work has started since we
                        // acquired the lock.)
                        if (_workQueue.Count == 0 && _runningThreads == 0)
                            // Ok, looks like we're done here. Signal GetResults()
                            _getResultsEvent.Set();
            }
        }

        #region Dispose pattern

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="AsyncWorkQueueTest.AsyncWorkQueue`1"/> is reclaimed by garbage collection.
        /// </summary>
        ~AsyncWorkQueue()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="AsyncWorkQueueTest.AsyncWorkQueue`1"/> object.
        /// </summary>
        /// <remarks>
        /// Call <see cref="Dispose"/> when you are finished using the <see cref="AsyncWorkQueueTest.AsyncWorkQueue`1"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="AsyncWorkQueueTest.AsyncWorkQueue`1"/> in an unusable state.
        /// After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="AsyncWorkQueueTest.AsyncWorkQueue`1"/> so the garbage collector can reclaim the memory that the
        /// <see cref="AsyncWorkQueueTest.AsyncWorkQueue`1"/> was occupying.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                // always close our semaphore so we can kill the threads.

                _workSync.Release(int.MaxValue);
                _workSync.Close();

                // Is stuff still running? Better make sure it's dead.
                if (_runningThreads > 0)
                    foreach (Thread thread in _threads)
                        thread.Abort();
            }
        }

        #endregion

    }
}

#pragma warning restore 0420
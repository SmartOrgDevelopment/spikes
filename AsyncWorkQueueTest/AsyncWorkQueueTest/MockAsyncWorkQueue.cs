using System;
using System.Collections.Generic;

namespace AsyncWorkQueueTest
{
    public class MockAsyncWorkQueue<T> : IAsyncWorkQueue<T>
    {
        private readonly List<AsyncWorkResult<T>> _results = new List<AsyncWorkResult<T>>();

        public void Start(AsyncWorker<T> work)
        {
            try
            {
                _results.Add(work());
            }
            catch (Exception error)
            {
                _results.Add(new AsyncWorkResult<T>(error));
            }
        }

        public IEnumerable<AsyncWorkResult<T>> GetResults()
        {
            AsyncWorkResult<T>[] copy = new AsyncWorkResult<T>[_results.Count];
            _results.CopyTo(copy);
            _results.Clear();

            return copy;
        }
    }
}


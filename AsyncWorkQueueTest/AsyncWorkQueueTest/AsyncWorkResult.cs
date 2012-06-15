using System;

namespace AsyncWorkQueueTest
{
    /// <summary>
    /// A wrapper for the results of asynchronous execution via <see cref="AsyncWorkQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of result.</typeparam>
    public sealed class AsyncWorkResult<T>
    {
        private readonly bool _isError;
        private readonly Exception _error;
        private readonly T _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncWorkResult&lt;T&gt;"/> class as an error.
        /// </summary>
        /// <param name="error">The error.</param>
        public AsyncWorkResult(Exception error)
        {
            _isError = true;
            _error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncWorkResult&lt;T&gt;"/> class as a successful value.
        /// </summary>
        /// <param name="result">The result.</param>
        public AsyncWorkResult(T result)
        {
            _result = result;
        }

        /// <summary>
        /// Gets a value indicating whether the result is an error.
        /// </summary>
        public bool IsError { get { return _isError; } }

        /// <summary>
        /// Gets the error that ocurred during execution.
        /// </summary>
        public Exception Error
        {
            get
            {
                if (!_isError)
                    throw new InvalidOperationException("Result is not an error.");

                return _error;
            }
        }

        /// <summary>
        /// Gets the successful result value.
        /// </summary>
        public T Value
        {
            get
            {
                if (_isError)
                    throw new InvalidOperationException("Result is an error.");

                return _result;
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T"/> to <see cref="AsyncWorkQueueTest.AsyncWorkResult&lt;T&gt;"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator AsyncWorkResult<T>(T value)
        {
            return new AsyncWorkResult<T>(value);
        }
    }
}
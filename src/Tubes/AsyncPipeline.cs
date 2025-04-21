using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tubes
{
    public interface IAsyncPipeline<TMessage>
    {
        AsyncPipeline<TMessage> Register(Func<TMessage, CancellationToken, Task> filter);
        Task ExecuteAsync(TMessage message, CancellationToken cancellationToken = default);
    }

    public sealed class AsyncPipeline<TMessage> : IAsyncPipeline<TMessage>
    {
        private readonly List<Func<TMessage, CancellationToken, Task>> _filters;

        public AsyncPipeline()
        {
            _filters = new List<Func<TMessage, CancellationToken, Task>>();
        }

        internal AsyncPipeline(List<Func<TMessage, CancellationToken, Task>> filters)
        {
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
        }

        public AsyncPipeline<TMessage> Register(Func<TMessage, CancellationToken, Task> filter)
        {
            _filters.Add(filter ?? throw new ArgumentNullException(nameof(filter)));
            return this;
        }

        public async Task ExecuteAsync(TMessage message, CancellationToken cancellationToken = default)
        {
            if(message == null)
                throw new ArgumentNullException(nameof(message));

            foreach (var filter in _filters)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                if(message is IStopProcessing stopProcessing && stopProcessing.Stop)
                    break;

                await filter(message, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
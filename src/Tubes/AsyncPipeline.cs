using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tubes;

public interface IAsyncPipeline<TMessage>
{
    AsyncPipeline<TMessage> Register(IAsyncFilter<TMessage> filter);
    Task ExecuteAsync(TMessage message, CancellationToken cancellationToken = default);
}

public sealed class AsyncPipeline<TMessage> : IAsyncPipeline<TMessage>
{
    private readonly List<IAsyncFilter<TMessage>> _filters;

    public AsyncPipeline()
    {
        _filters = [];
    }

    internal AsyncPipeline(List<IAsyncFilter<TMessage>> filters)
    {
        _filters = filters ?? throw new ArgumentNullException(nameof(filters));
    }

    public AsyncPipeline<TMessage> Register(IAsyncFilter<TMessage> filter)
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
                
            if(message is IStopProcessing { Stop: true })
                break;

            await filter.ExecuteAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }
}
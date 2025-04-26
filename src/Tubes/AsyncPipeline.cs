using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Tubes;

public interface IAsyncPipeline<TMessage>
{
    AsyncPipeline<TMessage> Register<TFilter>()  where TFilter : IAsyncFilter<TMessage>;
    AsyncPipeline<TMessage> Register(IAsyncFilter<TMessage> filter);
    Task ExecuteAsync(TMessage message, CancellationToken cancellationToken = default);
}

public sealed class AsyncPipeline<TMessage> : IAsyncPipeline<TMessage>
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly List<IAsyncFilter<TMessage>> _filters = [];

    public AsyncPipeline()
    {
    }
    
    public AsyncPipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    
    public AsyncPipeline(List<IAsyncFilter<TMessage>> filters)
    {
        _filters = filters ?? throw new ArgumentNullException(nameof(filters));
    }

    public AsyncPipeline<TMessage> Register(IAsyncFilter<TMessage> filter)
    {
        _filters.Add(filter ?? throw new ArgumentNullException(nameof(filter)));
        return this;
    }
    
    public AsyncPipeline<TMessage> Register<TFilter>() where TFilter : IAsyncFilter<TMessage>
    {
        if(_serviceProvider is null)
            throw new Exception("Service provider is not available. Did you construct the pipeline with a service provider?");
        
        var filter = _serviceProvider.GetService<TFilter>() 
                     ?? throw new Exception($"Filter '{typeof(TFilter).Name}' is not registered with the service provider.");
        
        _filters.Add(filter);
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
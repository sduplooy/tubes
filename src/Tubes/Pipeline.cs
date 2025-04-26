using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Tubes;

public interface IPipeline<TMessage>
{
    Pipeline<TMessage> Register<TFilter>() where TFilter : IFilter<TMessage>;
    Pipeline<TMessage> Register(IFilter<TMessage> filter);
    void Process(TMessage message);
}

public sealed class Pipeline<TMessage> : IPipeline<TMessage>
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly List<IFilter<TMessage>> _filters = [];

    public Pipeline()
    {
    }
    
    public Pipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    
    internal Pipeline(List<IFilter<TMessage>> filters)
    {
        _filters = filters ?? throw new ArgumentNullException(nameof(filters));
    }
    
    public Pipeline<TMessage> Register(IFilter<TMessage> filter)
    {
        _filters.Add(filter ?? throw new ArgumentNullException(nameof(filter)));
        return this;
    }

    public Pipeline<TMessage> Register<TFilter>() where TFilter : IFilter<TMessage>
    {
        if(_serviceProvider is null)
            throw new Exception("Service provider is not available. Did you construct the pipeline with a service provider?");
        
        var filter = _serviceProvider.GetService<TFilter>() 
            ?? throw new Exception($"Filter '{typeof(TFilter).Name}' is not registered with the service provider.");
        
        _filters.Add(filter);
        return this;
    }
    
    public void Process(TMessage message)
    {
        if(message == null)
            throw new ArgumentNullException(nameof(message));

        _filters.ForEach(f =>
        {
            if (message is IStopProcessing { Stop: true })
                return;

            f.Process(message);
        });
    }
}
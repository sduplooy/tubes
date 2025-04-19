namespace Tubes;

public sealed class AsyncPipeline<TMessage>
{
    private readonly List<Action<TMessage, CancellationToken>> _filters;

    public AsyncPipeline()
    {
        _filters = [];
    }

    internal AsyncPipeline(List<Action<TMessage, CancellationToken>> filters)
    {
        _filters = filters ?? throw new ArgumentNullException(nameof(filters));
    }
    
    public AsyncPipeline<TMessage> Register(Action<TMessage, CancellationToken> filter)
    {
        _filters.Add(filter ?? throw new ArgumentNullException(nameof(filter)));
        return this;
    }
    
    public void ExecuteAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        
        foreach (var filter in _filters)
        {
            if (cancellationToken.IsCancellationRequested || message is IStopProcessing { Stop: true })
                break;
            
            filter(message, cancellationToken);
        }
    }
}
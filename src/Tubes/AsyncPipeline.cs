namespace Tubes;

public sealed class AsyncPipeline<TMessage>
{
    private readonly List<Func<TMessage, CancellationToken, Task>> _filters;

    public AsyncPipeline()
    {
        _filters = [];
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
        ArgumentNullException.ThrowIfNull(message);
        
        foreach (var filter in _filters)
        {
            if (cancellationToken.IsCancellationRequested || message is IStopProcessing { Stop: true })
                break;
            
            await filter(message, cancellationToken).ConfigureAwait(false);
        }
    }
}
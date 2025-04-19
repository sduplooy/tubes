namespace Tubes.Aspects;

public record struct RetryOptions
{
    public RetryOptions(int maxRetries, TimeSpan slideTime)
    {
        if(maxRetries < 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries must be greater than 0.");
        
        MaxRetries = maxRetries;
        SlideTime = slideTime;
    }
    
    public int MaxRetries { get; }
    public TimeSpan SlideTime { get; }
}

public sealed class RetryAspect<TMessage>(Action<TMessage, CancellationToken> next, RetryOptions retryOptions)
{
    private int _currentRetry;
    
    public void Execute(TMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            if (cancellationToken.IsCancellationRequested) 
                return;
            
            next(message, cancellationToken);
        }
        catch (Exception)
        {
            _currentRetry++;
            
            if (_currentRetry <= retryOptions.MaxRetries)
            {
                Thread.Sleep(retryOptions.SlideTime * _currentRetry);
                Execute(message, cancellationToken);
            }
            else 
                throw;
        }
    }
}
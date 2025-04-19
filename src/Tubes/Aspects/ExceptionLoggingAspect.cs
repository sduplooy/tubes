using Microsoft.Extensions.Logging;

namespace Tubes.Aspects;

public sealed class ExceptionLoggingAspect<T>(Action<T, CancellationToken> next, ILogger<ExceptionLoggingAspect<T>> logger)
{
    public void Execute(T message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(message);
        
        try
        {
            if (cancellationToken.IsCancellationRequested) 
                return;
            
            next(message, cancellationToken);
        }
        catch (Exception ex)
        {
            if(logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "Exception in {Handler}: {Message}", typeof(T).Name, message.GetType().Name);
            
            throw;
        }
    }
}
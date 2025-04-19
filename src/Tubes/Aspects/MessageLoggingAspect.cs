using Microsoft.Extensions.Logging;

namespace Tubes.Aspects;

public sealed class MessageLoggingAspect<TMessage>(Action<TMessage, CancellationToken> next, ILogger<MessageLoggingAspect<TMessage>> logger)
{
    public void Execute(TMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(message);

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Message received: {Message}", message.GetType().Name);

        if (cancellationToken.IsCancellationRequested) 
            return;
        
        next(message, cancellationToken);
    }
}
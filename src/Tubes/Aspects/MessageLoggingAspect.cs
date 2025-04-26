using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Tubes.Aspects;

public sealed class MessageLoggingAspect<TMessage>(Action<TMessage, CancellationToken> next, ILogger<MessageLoggingAspect<TMessage>> logger)
{
    private readonly Action<TMessage, CancellationToken> _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly ILogger<MessageLoggingAspect<TMessage>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public void Execute(TMessage message, CancellationToken cancellationToken = default)
    {
        if(message == null)
            throw new ArgumentNullException(nameof(message));
            
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Message received: {Message}", message.GetType().Name);

        if (cancellationToken.IsCancellationRequested)
            return;

        _next(message, cancellationToken);
    }
}
using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Tubes.Aspects
{
    public sealed class MessageLoggingAspect<TMessage>
    {
        private readonly Action<TMessage, CancellationToken> _next;
        private readonly ILogger<MessageLoggingAspect<TMessage>> _logger;

        public MessageLoggingAspect(Action<TMessage, CancellationToken> next, ILogger<MessageLoggingAspect<TMessage>> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
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
}
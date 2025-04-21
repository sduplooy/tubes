using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Tubes.Aspects
{
    public sealed class ExceptionLoggingAspect<T>
    {
        private readonly Action<T, CancellationToken> _next;
        private readonly ILogger<ExceptionLoggingAspect<T>> _logger;

        public ExceptionLoggingAspect(Action<T, CancellationToken> next, ILogger<ExceptionLoggingAspect<T>> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public void Execute(T message, CancellationToken cancellationToken = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                _next(message, cancellationToken);
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(ex, "Exception in {Handler}: {Message}", typeof(T).Name, message.GetType().Name);

                throw;
            }
        }
    }
}
using System;
using System.Threading;

namespace Tubes.Aspects
{
    public class RetryOptions
    {
        public RetryOptions(int maxRetries, TimeSpan slideTime)
        {
            if (maxRetries < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries must be greater than 0.");

            MaxRetries = maxRetries;
            SlideTime = slideTime;
        }

        public int MaxRetries { get; }
        public TimeSpan SlideTime { get; }
    }
    
    public sealed class RetryAspect<TMessage>
    {
        private readonly Action<TMessage, CancellationToken> _next;
        private readonly RetryOptions _retryOptions;
        private int _currentRetry;

        public RetryAspect(Action<TMessage, CancellationToken> next, RetryOptions retryOptions)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _retryOptions = retryOptions ?? throw new ArgumentNullException(nameof(retryOptions));
        }
        
        public void Execute(TMessage message, CancellationToken cancellationToken = default)
        {
            if(message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                _next(message, cancellationToken);
            }
            catch (Exception)
            {
                _currentRetry++;

                if (_currentRetry <= _retryOptions.MaxRetries)
                {
                    Thread.Sleep(_retryOptions.SlideTime.Milliseconds * _currentRetry);
                    Execute(message, cancellationToken);
                }
                else
                    throw;
            }
        }
    }
}
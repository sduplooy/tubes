using System;
using System.Threading;
using System.Transactions;

namespace Tubes.Aspects;

public class TransactionAspect<TMessage>(Action<TMessage, CancellationToken> next)
{
    private readonly Action<TMessage, CancellationToken> _next = next ?? throw new ArgumentNullException(nameof(next));

    public void Execute(TMessage message, CancellationToken cancellationToken = default)
    {
        if(message == null)
            throw new ArgumentNullException(nameof(message));
            
        if (cancellationToken.IsCancellationRequested)
            return;

        using var scope = new TransactionScope();
        _next(message, cancellationToken);
        scope.Complete();
    }
}
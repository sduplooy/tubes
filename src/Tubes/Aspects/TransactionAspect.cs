using System.Transactions;

namespace Tubes.Aspects;

public class TransactionAspect<TMessage>(Action<TMessage, CancellationToken> next)
{
    public void Execute(TMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(message);

        if (cancellationToken.IsCancellationRequested) 
            return;
        
        using var scope = new TransactionScope();
        next(message, cancellationToken);
        scope.Complete();
    }
}

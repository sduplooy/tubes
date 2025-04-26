using System.Threading;
using System.Threading.Tasks;

namespace Tubes;

public interface IAsyncFilter<in TMessage>
{
    Task ExecuteAsync(TMessage message, CancellationToken cancellationToken = default);
}

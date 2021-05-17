using System.Threading;
using System.Threading.Tasks;

namespace Common.Infrastructure.Processing
{
    public interface IDomainEventsDispatcher
    {
        Task DispatchEventsAsync(CancellationToken cancellationToken = default);
    }
}

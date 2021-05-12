using System.Threading;
using System.Threading.Tasks;

namespace Character.Api.Infrastructure.Processing
{
    public interface IDomainEventsDispatcher
    {
        Task DispatchEventsAsync(CancellationToken cancellationToken = default);
    }
}

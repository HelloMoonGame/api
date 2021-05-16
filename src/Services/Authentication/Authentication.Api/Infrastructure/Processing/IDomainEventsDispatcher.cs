using System.Threading;
using System.Threading.Tasks;

namespace Authentication.Api.Infrastructure.Processing
{
    public interface IDomainEventsDispatcher
    {
        Task DispatchEventsAsync(CancellationToken cancellationToken = default);
    }
}

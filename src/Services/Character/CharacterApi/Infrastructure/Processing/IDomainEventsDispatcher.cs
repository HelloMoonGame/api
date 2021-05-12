using System.Threading;
using System.Threading.Tasks;

namespace CharacterApi.Infrastructure.Processing
{
    public interface IDomainEventsDispatcher
    {
        Task DispatchEventsAsync(CancellationToken cancellationToken = default);
    }
}

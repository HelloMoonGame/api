using System.Threading;
using System.Threading.Tasks;
using CharacterApi.Domain.CharacterLocations;
using CharacterApi.GrpcServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CharacterApi.Application.CharacterLocations.IntegrationHandlers
{
    public class CharacterMovedDomainEventHandler : INotificationHandler<CharacterMovedEvent>
    {
        private readonly ILogger<CharacterMovedDomainEventHandler> _logger;

        public CharacterMovedDomainEventHandler(ILogger<CharacterMovedDomainEventHandler> logger)
        {
            _logger = logger;
        }
        
        public Task Handle(CharacterMovedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Character {notification.CharacterId} moved to {notification.ToX},{notification.ToY}");
            LocationService.PublishCharacterLocation(notification.CharacterId, new CharacterLocationDto
            {
                CharacterId = notification.CharacterId,
                X = notification.ToX,
                Y = notification.ToY
            }, cancellationToken);
            
            return Task.CompletedTask;
        }
    }
}
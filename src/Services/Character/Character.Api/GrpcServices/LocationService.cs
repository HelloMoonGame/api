using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CharacterApi.Application.CharacterLocations;
using CharacterApi.Application.CharacterLocations.GetCharacterLocation;
using CharacterApi.Application.CharacterLocations.GetCharacterLocations;
using CharacterApi.Application.CharacterLocations.SpawnCharacter;
using CharacterApi.Application.Characters.GetUserCharacter;
using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CharacterApi.GrpcServices
{
    [Authorize]
    public class LocationService : Location.LocationBase
    {
        private static readonly ConcurrentDictionary<Guid, IList<IServerStreamWriter<LocationUpdateResponse>>> Subscriptions = new();

        private readonly ILogger<LocationService> _logger;
        private readonly IMediator _mediator;

        public LocationService(ILogger<LocationService> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        private void AddSubscription(Guid characterId, IServerStreamWriter<LocationUpdateResponse> responseStream)
        {
            lock (Subscriptions)
            {
                if (Subscriptions.TryGetValue(characterId, out var subscriptions))
                {
                    subscriptions.Add(responseStream);
                    _logger.LogInformation($"New connection for character: {characterId}");
                }
                else
                {
                    Subscriptions.TryAdd(characterId,
                        new List<IServerStreamWriter<LocationUpdateResponse>> { responseStream });
                    _logger.LogInformation($"New character online: {characterId}");
                }
            }
        }

        private void RemoveSubscription(Guid characterId, IServerStreamWriter<LocationUpdateResponse> responseStream, CancellationToken cancellationToken)
        {
            lock (Subscriptions)
            {
                if (Subscriptions.TryGetValue(characterId, out var subscriptions))
                {
                    subscriptions.Remove(responseStream);
                    if (subscriptions.Count == 0)
                    {
                        _logger.LogInformation($"Character {characterId} is disconnected and will be removed from subscription list");
                        if (!Subscriptions.TryRemove(characterId, out _))
                            _logger.LogWarning($"Could not remove character {characterId} from subscription list");

                        PublishCharacterLocation(characterId, null, cancellationToken);
                    }
                }
            }
        }

        public static void PublishCharacterLocation(Guid characterId, CharacterLocationDto location, CancellationToken cancellationToken)
        {
            var message = new LocationUpdateResponse
            {
                LocationUpdates =
                {
                    new LocationUpdate {
                        CharacterId = characterId.ToString(),
                        Online = location != null,
                        X = location?.X ?? 0,
                        Y = location?.Y ?? 0
                    }
                }
            };

            lock (Subscriptions)
                Task.WaitAll(Subscriptions.Values
                    .SelectMany(x => x)
                    .Select(s => s.WriteAsync(message))
                    .ToArray(), cancellationToken);
        }

        public override async Task Subscribe(Empty request, IServerStreamWriter<LocationUpdateResponse> responseStream, ServerCallContext context)
        {
            var userId = GetUserId(context);

            var character = await _mediator.Send(new GetUserCharacterQuery(userId), context.CancellationToken);
            if (character == null)
                return;
            
            var characterLocation = await _mediator.Send(new GetCharacterLocationQuery(character.Id), context.CancellationToken) ??
                                    await _mediator.Send(new SpawnCharacterCommand(character.Id), context.CancellationToken);

            PublishCharacterLocation(characterLocation.CharacterId, characterLocation, context.CancellationToken);
            
            AddSubscription(character.Id, responseStream);
            
            var message = new LocationUpdateResponse();
            if (Subscriptions != null)
            {
                List<Guid> onlineCharacterIds;
                lock (Subscriptions)
                {
                    onlineCharacterIds = Subscriptions.Keys.ToList();
                }

                var onlineCharacters = await _mediator.Send(
                        new GetCharacterLocationsQuery(onlineCharacterIds),
                        context.CancellationToken
                    );

                message.LocationUpdates.AddRange(onlineCharacters
                    .Select(c => new LocationUpdate
                    {
                        CharacterId = c.CharacterId.ToString(),
                        Online = true,
                        X = c.X,
                        Y = c.Y
                    }));
            }

            await responseStream.WriteAsync(message);

            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                RemoveSubscription(character.Id, responseStream, CancellationToken.None);
            }
        }

        private static Guid GetUserId(ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userGuid = Guid.Parse(userId);
            return userGuid;
        }
    }
}

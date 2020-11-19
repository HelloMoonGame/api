using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CharacterApi.Models;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace CharacterApi.Services
{
    public class LocationService : Location.LocationBase
    {
        private static readonly ConcurrentDictionary<Guid, IList<IServerStreamWriter<LocationUpdateResponse>>> Subscriptions = new ();

        private static readonly ConcurrentDictionary<Guid, CharacterLocation> CharacterLocations = new ();

        public static IReadOnlyList<CharacterInfo> Characters =>
            new ReadOnlyCollection<CharacterInfo>(CharacterLocations.Select(x => new CharacterInfo(x.Key, x.Value.X, x.Value.Y)).ToList());

        private readonly ILogger<LocationService> _logger;
        public LocationService(ILogger<LocationService> logger)
        {
            _logger = logger;
        }

        private void AddSubscription(Guid characterGuid, IServerStreamWriter<LocationUpdateResponse> responseStream)
        {
            lock (Subscriptions)
            {
                if (Subscriptions.TryGetValue(characterGuid, out var subscriptions))
                {
                    subscriptions.Add(responseStream);
                    _logger.LogInformation($"New connection for character: {characterGuid}");
                }
                else
                {
                    Subscriptions.TryAdd(characterGuid, new List<IServerStreamWriter<LocationUpdateResponse>> { responseStream });
                    _logger.LogInformation($"New character online: {characterGuid}");
                    UpdateCharacterLocation(characterGuid, GenerateCharacterLocation());
                }
            }
        }

        private static CharacterLocation GenerateCharacterLocation()
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            return new CharacterLocation(random.Next(-2, 2), random.Next(-2, 2));
        }

        private void RemoveSubscription(Guid characterGuid, IServerStreamWriter<LocationUpdateResponse> responseStream)
        {
            lock (Subscriptions)
            {
                if (Subscriptions.TryGetValue(characterGuid, out var subscriptions))
                {
                    subscriptions.Remove(responseStream);
                    if (subscriptions.Count == 0)
                    {
                        _logger.LogInformation($"Character {characterGuid} is disconnected and will be removed from subscription list");
                        if (!Subscriptions.TryRemove(characterGuid, out _))
                            _logger.LogWarning($"Could not remove character {characterGuid} from subscription list");
                        
                        UpdateCharacterLocation(characterGuid, null);
                    }
                }
            }
        }

        private void UpdateCharacterLocation(Guid characterGuid, CharacterLocation newLocation)
        {
            if (newLocation != null)
            {
                CharacterLocations.AddOrUpdate(characterGuid, newLocation, (guid, oldLocation) => newLocation);
                _logger.LogInformation($"Character {characterGuid} is now at location {newLocation.X},{newLocation.Y}");
            }
            else
            {
                if (!CharacterLocations.TryRemove(characterGuid, out _))
                    _logger.LogWarning($"Could not remove character {characterGuid} from location list");
            }

            var message = new LocationUpdateResponse
            {
                LocationUpdates =
                {
                    new LocationUpdate {
                        CharacterId = characterGuid.ToString(),
                        Online = newLocation != null,
                        X = newLocation?.X ?? 0,
                        Y = newLocation?.Y ?? 0
                    }
                }
            };

            Task.WaitAll(Subscriptions.Values
                .SelectMany(x => x)
                .Select(s => s.WriteAsync(message))
                .ToArray());
        }

        public override async Task Subscribe(Empty request, IServerStreamWriter<LocationUpdateResponse> responseStream, ServerCallContext context)
        {
            var characterGuid = Guid.NewGuid();

            AddSubscription(characterGuid, responseStream);

            var message = new LocationUpdateResponse();
            lock (CharacterLocations)
            {
                message.LocationUpdates.AddRange(CharacterLocations.Select(c => new LocationUpdate
                {
                    CharacterId = c.Key.ToString(),
                    Online = true,
                    X = c.Value.X,
                    Y = c.Value.Y
                }));
            }
            await responseStream.WriteAsync(message);

            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                RemoveSubscription(characterGuid, responseStream);
            }
        }
    }
}

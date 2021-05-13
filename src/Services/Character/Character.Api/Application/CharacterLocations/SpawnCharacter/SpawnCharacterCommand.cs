using System;
using MediatR;

namespace Character.Api.Application.CharacterLocations.SpawnCharacter
{
    public class SpawnCharacterCommand : IRequest<CharacterLocationDto>
    {
        public Guid CharacterId { get; }

        public SpawnCharacterCommand(Guid characterId)
        {
            CharacterId = characterId;
        }
    }
}

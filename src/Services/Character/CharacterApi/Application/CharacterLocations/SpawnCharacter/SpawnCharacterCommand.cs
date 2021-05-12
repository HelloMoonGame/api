using System;
using MediatR;

namespace CharacterApi.Application.CharacterLocations.SpawnCharacter
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

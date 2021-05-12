using System;
using MediatR;

namespace CharacterApi.Application.CharacterLocations.GetCharacterLocation
{
    public class GetCharacterLocationQuery : IRequest<CharacterLocationDto>
    {
        public GetCharacterLocationQuery(Guid characterId)
        {
            CharacterId = characterId;
        }

        public Guid CharacterId { get; }
    }
}
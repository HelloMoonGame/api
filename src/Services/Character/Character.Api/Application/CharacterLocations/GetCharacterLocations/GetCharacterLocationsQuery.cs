using System;
using System.Collections.Generic;
using MediatR;

namespace CharacterApi.Application.CharacterLocations.GetCharacterLocations
{
    public class GetCharacterLocationsQuery : IRequest<IEnumerable<CharacterLocationDto>>
    {
        public GetCharacterLocationsQuery(IEnumerable<Guid> characterIds)
        {
            CharacterIds = characterIds;
        }

        public IEnumerable<Guid> CharacterIds { get; }
    }
}
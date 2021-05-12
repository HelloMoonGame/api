using System;
using System.Collections.Generic;
using MediatR;

namespace Character.Api.Application.CharacterLocations.GetCharacterLocations
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
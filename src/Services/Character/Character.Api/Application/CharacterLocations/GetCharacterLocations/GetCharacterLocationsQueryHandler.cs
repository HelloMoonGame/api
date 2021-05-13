using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Character.Api.Domain.CharacterLocations;
using MediatR;

namespace Character.Api.Application.CharacterLocations.GetCharacterLocations
{
    public class GetCharacterLocationsQueryHandler : IRequestHandler<GetCharacterLocationsQuery, IEnumerable<CharacterLocationDto>>
    {
        private readonly ICharacterLocationRepository _characterLocationRepository;

        public GetCharacterLocationsQueryHandler(ICharacterLocationRepository characterLocationRepository)
        {
            _characterLocationRepository = characterLocationRepository;
        }

        public async Task<IEnumerable<CharacterLocationDto>> Handle(GetCharacterLocationsQuery request, CancellationToken cancellationToken)
        {
            var characterLocations = await _characterLocationRepository.GetByCharacterIdsAsync(request.CharacterIds, cancellationToken);
            return characterLocations?.Select(CharacterLocationDto.FromCharacterLocation);
        }
    }
}
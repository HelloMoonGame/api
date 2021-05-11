using System.Threading;
using System.Threading.Tasks;
using CharacterApi.Domain.CharacterLocations;
using MediatR;

namespace CharacterApi.Application.CharacterLocations.GetCharacterLocation
{
    public class GetCharacterLocationQueryHandler : IRequestHandler<GetCharacterLocationQuery, CharacterLocationDto>
    {
        private readonly ICharacterLocationRepository _characterLocationRepository;

        public GetCharacterLocationQueryHandler(ICharacterLocationRepository characterLocationRepository)
        {
            _characterLocationRepository = characterLocationRepository;
        }

        public async Task<CharacterLocationDto> Handle(GetCharacterLocationQuery request, CancellationToken cancellationToken)
        {
            var characterLocation = await _characterLocationRepository.GetByCharacterIdAsync(request.CharacterId, cancellationToken);
            return characterLocation == null ? null : CharacterLocationDto.FromCharacterLocation(characterLocation);
        }
    }
}
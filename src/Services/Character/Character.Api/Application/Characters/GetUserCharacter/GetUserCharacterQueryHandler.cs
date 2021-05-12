using System.Threading;
using System.Threading.Tasks;
using Character.Api.Domain.Characters;
using MediatR;

namespace Character.Api.Application.Characters.GetUserCharacter
{
    public class GetUserCharacterQueryHandler : IRequestHandler<GetUserCharacterQuery, CharacterDto>
    {
        private readonly ICharacterRepository _characterRepository;

        public GetUserCharacterQueryHandler(ICharacterRepository characterRepository)
        {
            _characterRepository = characterRepository;
        }

        public async Task<CharacterDto> Handle(GetUserCharacterQuery request, CancellationToken cancellationToken)
        {
            var character = await _characterRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            return character == null ? null : CharacterDto.FromCharacter(character);
        }
    }
}
using System.Threading;
using System.Threading.Tasks;
using Character.Api.Domain.CharacterLocations;
using Common.Domain.SeedWork;
using MediatR;

namespace Character.Api.Application.CharacterLocations.MoveCharacter
{
    public class MoveCharacterCommandHandler : IRequestHandler<MoveCharacterCommand, CharacterLocationDto>
    {
        private readonly ICharacterLocationRepository _characterLocationRepository;
        private readonly ISingleLocationPerCharacterChecker _singleLocationPerCharacterChecker;
        private readonly IUnitOfWork _unitOfWork;

        public MoveCharacterCommandHandler(
            ICharacterLocationRepository characterLocationRepository,
            ISingleLocationPerCharacterChecker singleLocationPerCharacterChecker,
            IUnitOfWork unitOfWork)
        {
            _characterLocationRepository = characterLocationRepository;
            _singleLocationPerCharacterChecker = singleLocationPerCharacterChecker;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<CharacterLocationDto> Handle(MoveCharacterCommand request, CancellationToken cancellationToken)
        {
            var characterLocation = await _characterLocationRepository.GetByCharacterIdAsync(request.CharacterId, cancellationToken);
            if (characterLocation != null)
            {
                characterLocation.ChangeLocation(request.X, request.Y);
            }
            else
            {
                characterLocation = CharacterLocation.Create(request.CharacterId, 
                    request.X, request.Y, _singleLocationPerCharacterChecker);
                await _characterLocationRepository.AddAsync(characterLocation, cancellationToken);
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return CharacterLocationDto.FromCharacterLocation(characterLocation);
        }
    }
}

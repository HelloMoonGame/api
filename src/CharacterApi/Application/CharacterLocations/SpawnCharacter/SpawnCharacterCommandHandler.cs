using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CharacterApi.Domain.CharacterLocations;
using CharacterApi.Domain.SeedWork;
using CharacterApi.Domain.SharedKernel;

namespace CharacterApi.Application.CharacterLocations.SpawnCharacter
{
    public class SpawnCharacterCommandHandler : IRequestHandler<SpawnCharacterCommand, CharacterLocationDto>
    {
        private readonly ICharacterLocationRepository _characterLocationRepository;
        private readonly ISingleLocationPerCharacterChecker _singleLocationPerCharacterChecker;
        private readonly IUnitOfWork _unitOfWork;

        public SpawnCharacterCommandHandler(
            ICharacterLocationRepository characterLocationRepository,
            ISingleLocationPerCharacterChecker singleLocationPerCharacterChecker,
            IUnitOfWork unitOfWork)
        {
            _characterLocationRepository = characterLocationRepository;
            _singleLocationPerCharacterChecker = singleLocationPerCharacterChecker;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<CharacterLocationDto> Handle(SpawnCharacterCommand request, CancellationToken cancellationToken)
        {
            var random = new Random(SystemClock.Now.Millisecond);
            var characterLocation = CharacterLocation.Create(request.CharacterId, random.Next(-2, 2), random.Next(-2, 2), _singleLocationPerCharacterChecker);

            await _characterLocationRepository.AddAsync(characterLocation, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return CharacterLocationDto.FromCharacterLocation(characterLocation);
        }
    }
}

using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CharacterApi.Domain.Characters;
using CharacterApi.Domain.SeedWork;

namespace CharacterApi.Application.Characters.CreateCharacter
{
    public class CreateCharacterCommandHandler : IRequestHandler<CreateCharacterCommand, CharacterDto>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ISingleCharacterPerUserChecker _singleCharacterPerUserChecker;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCharacterCommandHandler(
            ICharacterRepository characterRepository,
            ISingleCharacterPerUserChecker singleCharacterPerUserChecker,
            IUnitOfWork unitOfWork)
        {
            _characterRepository = characterRepository;
            _singleCharacterPerUserChecker = singleCharacterPerUserChecker;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<CharacterDto> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
        {
            var character = Character.Create(request.UserId, request.FirstName, request.LastName, request.Sex, _singleCharacterPerUserChecker);

            await _characterRepository.AddAsync(character, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return new CharacterDto { Id = character.Id };
        }
    }
}

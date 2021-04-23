using System;
using MediatR;

namespace CharacterApi.Application.Characters.GetUserCharacter
{
    public class GetUserCharacterQuery : IRequest<CharacterDto>
    {
        public GetUserCharacterQuery(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}
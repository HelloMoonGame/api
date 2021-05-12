using System;
using CharacterApi.Domain.Characters;
using MediatR;

namespace CharacterApi.Application.Characters.CreateCharacter
{
    public class CreateCharacterCommand : IRequest<CharacterDto>
    {
        public Guid UserId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public SexType Sex { get; }

        public CreateCharacterCommand(Guid userId, string firstName, string lastName, SexType sex)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            Sex = sex;
        }
    }
}

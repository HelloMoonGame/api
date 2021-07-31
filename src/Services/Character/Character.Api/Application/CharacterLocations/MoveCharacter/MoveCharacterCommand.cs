using System;
using MediatR;

namespace Character.Api.Application.CharacterLocations.MoveCharacter
{
    public class MoveCharacterCommand : IRequest<CharacterLocationDto>
    {
        public Guid CharacterId { get; }
        
        public int X { get; set; }
        
        public int Y { get; set; }

        public MoveCharacterCommand(Guid characterId, int x, int y)
        {
            CharacterId = characterId;
            X = x;
            Y = y;
        }
    }
}

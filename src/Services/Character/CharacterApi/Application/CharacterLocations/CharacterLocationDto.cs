using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CharacterApi.Application.Characters;
using CharacterApi.Domain.CharacterLocations;
using CharacterApi.Domain.Characters;

namespace CharacterApi.Application.CharacterLocations
{
    /// <summary>
    /// Location information for a character
    /// </summary>
    public class CharacterLocationDto
    {
        /// <summary>
        /// Unique identifier of the character
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// X-coordinate of the character on the map
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// y-coordinate of the character on the map
        /// </summary>
        public int Y { get; set; }
        
        public static CharacterLocationDto FromCharacterLocation([NotNull] CharacterLocation characterLocation)
        {
            return new()
            {
                CharacterId = characterLocation.CharacterId,
                X = characterLocation.X,
                Y = characterLocation.Y
            };
        }
    }
}

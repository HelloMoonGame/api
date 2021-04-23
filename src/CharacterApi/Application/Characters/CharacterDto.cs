using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CharacterApi.Domain.Characters;

namespace CharacterApi.Application.Characters
{
    /// <summary>
    /// Character that is controlled by the user
    /// </summary>
    public class CharacterDto
    {
        /// <summary>
        /// Unique identifier of the character
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// First name of the character
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the character
        /// </summary>
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// Sex describes the biological attributes of the character
        /// </summary>
        public SexType Sex { get; set; }

        public static CharacterDto FromCharacter([NotNull] Character character)
        {
            return new()
            {
                Id = character.Id,
                FirstName = character.FirstName,
                LastName = character.LastName,
                Sex = character.Sex
            };
        }
    }
}

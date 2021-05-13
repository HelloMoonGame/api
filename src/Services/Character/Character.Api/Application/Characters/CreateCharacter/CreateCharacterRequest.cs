using Character.Api.Domain.Characters;

namespace Character.Api.Application.Characters.CreateCharacter
{
    public class CreateCharacterRequest
    {
        /// <summary>
        /// First Name of the new character
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last Name of the new character
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Sex describes the biological attributes of the new character
        /// </summary>
        public SexType Sex { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace Character.Api.Application.Travel.StartTravel
{
    public class StartTravelRequest
    {
        /// <summary>
        /// Id of the character to move
        /// </summary>
        [Required]
        public Guid CharacterId { get; set; }
        
        /// <summary>
        /// X-position of the lot to travel to
        /// </summary>
        [Required]
        public int X { get; set; }

        /// <summary>
        /// Y-position of the lot to travel to
        /// </summary>
        [Required]
        public int Y { get; set; }
    }
}

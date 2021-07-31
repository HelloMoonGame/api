using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Character.Api.Application.CharacterLocations.MoveCharacter;
using Character.Api.Application.Characters;
using Character.Api.Application.Characters.GetUserCharacter;
using Character.Api.Application.Travel.StartTravel;
using Character.Api.Domain.Characters.Rules;
using Common.Domain.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Character.Api.Controllers
{
    /// <summary>
    /// Move a character over the map
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("travel")]
    [Produces("application/json")]
    public class TravelController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TravelController> _logger;

        public TravelController(IMediator mediator, ILogger<TravelController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Travel to a given location
        /// </summary>
        /// <param name="request">Character and destination</param>
        /// <returns>Created travel job</returns>
        [Route("")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> StartTravel([FromBody] StartTravelRequest request)
        {
            var userId = Guid.Parse(User?.Identity?.Name ?? "");
            
            _logger.LogInformation("Try to move character {CharacterId} of user {User} to {X},{Y}",
                request.CharacterId, userId, request.X, request.Y);

            var character = await _mediator.Send(new GetUserCharacterQuery(userId));
            if (request.CharacterId != character.Id)
            {
                _logger.LogError("User {User} is playing with {CurrentCharacterId} and tried to travel with {CharacterId}", userId, character.Id, request.CharacterId);
                return BadRequest();
            }

            await _mediator.Send(new MoveCharacterCommand(request.CharacterId, request.X, request.Y));
            _logger.LogInformation("Character {CharacterId} moved to {X},{Y}", request.CharacterId, request.X, request.Y);
            return Created(string.Empty, null);
        }
    }
}

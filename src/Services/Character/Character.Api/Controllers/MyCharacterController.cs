using System;
using System.Net;
using System.Threading.Tasks;
using Character.Api.Application.Characters;
using Character.Api.Application.Characters.CreateCharacter;
using Character.Api.Application.Characters.GetUserCharacter;
using Character.Api.Domain.Characters.Rules;
using Common.Domain.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Character.Api.Controllers
{
    /// <summary>
    /// Manage the character a user is playing with.
    /// A user can only have one active character.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("mycharacter")]
    [Produces("application/json")]
    public class MyCharacterController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MyCharacterController> _logger;

        public MyCharacterController(IMediator mediator, ILogger<MyCharacterController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get the details of the character a user is playing with.
        /// </summary>
        /// <response code="200">Current user's character</response>
        /// <response code="404">If the user does not have a character</response>    
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(CharacterDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetMyCharacter()
        {
            var character = await _mediator.Send(new GetUserCharacterQuery(Guid.Parse(User?.Identity?.Name ?? "")));
            if (character == null)
            {
                _logger.LogInformation("No character found for user '{User}'", User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("Character {CharacterId} found for user '{User}'", character.Id, User?.Identity?.Name);
            return Ok(character);
        }

        /// <summary>
        /// Create a new character if the user currently doesn't have one.
        /// </summary>
        /// <param name="request">Character details</param>
        /// <returns>Created character</returns>
        [Route("")]
        [HttpPost]
        [ProducesResponseType(typeof(CharacterDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(CharacterDto), (int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterRequest request)
        {
            var userId = Guid.Parse(User?.Identity?.Name ?? "");

            _logger.LogInformation("Try to create character {firstName} {lastName} for user {User}", 
                request.FirstName, request.LastName, userId);
            try
            {
                var character = await _mediator.Send(new CreateCharacterCommand(userId,
                    request.FirstName, request.LastName, request.Sex));
                _logger.LogInformation("Character {CharacterId} created", character.Id);
                return Created(string.Empty, character);
            }
            catch(BusinessRuleValidationException e)
            {
                if (e.BrokenRule.GetType() == typeof(UserCanOnlyHaveOneCharacter))
                {
                    var existingCharacter = await _mediator.Send(new GetUserCharacterQuery(userId));
                    _logger.LogWarning("User {User} already has a character {FirstName} {LastName} ({CharacterId})",
                        userId, existingCharacter?.FirstName, existingCharacter?.LastName, existingCharacter?.Id);
                    return Conflict(existingCharacter);
                }

                throw;
            }
        }
    }
}

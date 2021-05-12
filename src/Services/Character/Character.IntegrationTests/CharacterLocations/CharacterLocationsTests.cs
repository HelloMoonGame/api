using System;
using System.Threading.Tasks;
using Character.Api.Application.CharacterLocations.SpawnCharacter;
using Character.Api.Application.Characters.CreateCharacter;
using Character.Api.Domain.CharacterLocations.Rules;
using Character.Api.Domain.Characters;
using Character.Api.Domain.SeedWork;
using Character.IntegrationTests.SeedWork;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Character.IntegrationTests.CharacterLocations
{
    [TestClass]
    public class CharacterLocationsTests : TestBase
    {
        [TestMethod]
        public async Task A_new_character_can_be_spawned()
        {
            var userId = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);

            var character = await mediator.Send(new CreateCharacterCommand(
                userId,
                "FirstName",
                "LastName",
                SexType.Male));

            var characterLocation = await mediator.Send(new SpawnCharacterCommand(character.Id));

            Assert.IsNotNull(characterLocation);
            Assert.AreEqual(character.Id, characterLocation.CharacterId);
        }
        
        [TestMethod]
        public async Task A_character_cannot_be_spawned_twice()
        {
            var userId = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);

            var character = await mediator.Send(new CreateCharacterCommand(
                userId,
                "FirstName",
                "LastName",
                SexType.Male));

            await mediator.Send(new SpawnCharacterCommand(character.Id));
            var exception = await Assert.ThrowsExceptionAsync<BusinessRuleValidationException>(() =>
                mediator.Send(new SpawnCharacterCommand(character.Id)));

            Assert.IsInstanceOfType(exception.BrokenRule, typeof(CharacterCanOnlyHaveOneLocation));
        }
    }
}

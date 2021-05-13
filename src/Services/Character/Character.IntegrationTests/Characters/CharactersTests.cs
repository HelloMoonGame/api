using System;
using System.Threading.Tasks;
using Character.Api.Application.Characters.CreateCharacter;
using Character.Api.Application.Characters.GetUserCharacter;
using Character.Api.Domain.Characters;
using Character.Api.Domain.Characters.Rules;
using Character.Api.Domain.SeedWork;
using Character.IntegrationTests.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Character.IntegrationTests.Characters
{
    [TestClass]
    public class CustomersTests : TestBase
    {
        [TestMethod]
        public async Task A_new_character_can_be_created()
        {
            var userId = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);
            
            var character = await mediator.Send(new CreateCharacterCommand(
                userId, 
                "FirstName", 
                "LastName", 
                SexType.Male));
            
            Assert.IsNotNull(character);
            Assert.AreEqual("FirstName", character.FirstName);
            Assert.AreEqual("LastName", character.LastName);
            Assert.AreEqual(SexType.Male, character.Sex);
        }
        
        [TestMethod]
        public async Task A_new_character_cannot_be_created_if_the_user_already_has_one()
        {
            var userId = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);

            await mediator.Send(new CreateCharacterCommand(
                userId,
                "FirstName",
                "LastName",
                SexType.Male));

            var exception = await Assert.ThrowsExceptionAsync<BusinessRuleValidationException>(() => 
                mediator.Send(new CreateCharacterCommand(
                    userId,
                    "FirstName2",
                    "LastName2",
                    SexType.Female)));
            
            Assert.IsInstanceOfType(exception.BrokenRule, typeof(UserCanOnlyHaveOneCharacter));
        }
        
        [TestMethod]
        public async Task Character_of_a_user_can_be_retrieved()
        {
            var userId = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);

            await mediator.Send(new CreateCharacterCommand(
                userId,
                "FirstName",
                "LastName",
                SexType.Male));

            var character = await mediator.Send(new GetUserCharacterQuery(userId));

            Assert.IsNotNull(character);
            Assert.AreEqual("FirstName", character.FirstName);
            Assert.AreEqual("LastName", character.LastName);
            Assert.AreEqual(SexType.Male, character.Sex);
        }

        [TestMethod]
        public async Task No_character_is_returned_if_user_does_not_have_a_character()
        {
            var userId = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);
            
            var character = await mediator.Send(new GetUserCharacterQuery(userId));

            Assert.IsNull(character);
        }

    }
}
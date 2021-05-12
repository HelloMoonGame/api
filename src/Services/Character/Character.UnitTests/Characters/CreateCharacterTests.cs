using System;
using System.Threading;
using System.Threading.Tasks;
using Character.Api.Application.Characters.DomainServices;
using Character.Api.Domain.Characters;
using Character.Api.Domain.Characters.Rules;
using Character.UnitTests.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Character.UnitTests.Characters
{
    [TestClass]
    public class CreateCharacterTests : TestBase
    {
        [TestMethod]
        public void User_can_create_a_character_if_it_has_no_living_character()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var characterRepository = new Mock<ICharacterRepository>();
            characterRepository
                .Setup(r => r.GetByUserIdAsync(
                    It.Is<Guid>(id => id == userId), 
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Api.Domain.Characters.Character>(null));

            var singleCharacterPerUserChecker = new SingleCharacterPerUserChecker(characterRepository.Object);


            // Act
            var character = Api.Domain.Characters.Character.Create(
                userId, 
                "Test", 
                "Test", 
                SexType.Female, 
                singleCharacterPerUserChecker
            );

            // Assert
            AssertPublishedDomainEvent<CharacterCreatedEvent>(character);
        }
        
        [TestMethod]
        public void User_cannot_create_a_second_character_if_another_is_still_alive()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var characterRepository = new Mock<ICharacterRepository>();
            var singleCharacterPerUserChecker = new SingleCharacterPerUserChecker(characterRepository.Object);

            var existingCharacter = Api.Domain.Characters.Character.Create(
                userId,
                "Test",
                "Test",
                SexType.Female,
                singleCharacterPerUserChecker
            );
            characterRepository
                .Setup(r => r.GetByUserIdAsync(
                    It.Is<Guid>(id => id == userId),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(existingCharacter));
            
            // Assert
            AssertBrokenRule<UserCanOnlyHaveOneCharacter>(() =>
            {
                // Act
                Api.Domain.Characters.Character.Create(
                    userId, 
                    "Tester", 
                    "Tester", 
                    SexType.Male, 
                    singleCharacterPerUserChecker);
            });
        }
    }
}

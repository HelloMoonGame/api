using System;
using System.Threading;
using System.Threading.Tasks;
using Character.Api.Application.CharacterLocations.DomainServices;
using Character.Api.Domain.CharacterLocations;
using Character.Api.Domain.CharacterLocations.Rules;
using Character.UnitTests.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Character.UnitTests.CharacterLocations
{
    [TestClass]
    public class SpawnCharacterTests : TestBase
    {
        [TestMethod]
        public void Character_can_be_spawned_if_it_has_no_location()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var characterLocationRepository = new Mock<ICharacterLocationRepository>();
            characterLocationRepository
                .Setup(r => r.GetByCharacterIdAsync(
                    It.Is<Guid>(id => id == characterId),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<CharacterLocation>(null));

            var singleLocationPerCharacterChecker = new SingleLocationPerCharacterChecker(characterLocationRepository.Object);

            // Act
            var character = CharacterLocation.Create(
                characterId,
                0,
                0,
                singleLocationPerCharacterChecker
            );

            // Assert
            AssertPublishedDomainEvent<CharacterMovedEvent>(character);
        }

        [TestMethod]
        public void Character_cannot_be_spawned_if_it_has_a_location()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var characterLocationRepository = new Mock<ICharacterLocationRepository>();
            var singleLocationPerCharacterChecker = new SingleLocationPerCharacterChecker(characterLocationRepository.Object);

            var existingCharacterLocation = CharacterLocation.Create(
                characterId,
                0,
                0,
                singleLocationPerCharacterChecker
            );

            characterLocationRepository
                .Setup(r => r.GetByCharacterIdAsync(
                    It.Is<Guid>(id => id == characterId),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(existingCharacterLocation));
            
            // Assert
            AssertBrokenRule<CharacterCanOnlyHaveOneLocation>(() =>
            {
                // Act
                CharacterLocation.Create(
                    characterId,
                    0,
                    0,
                    singleLocationPerCharacterChecker);
            });
        }
    }
}

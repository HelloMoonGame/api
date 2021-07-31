using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Character.Api.Application.CharacterLocations;
using Character.Api.Application.Characters;
using Character.Api.Application.Travel.StartTravel;
using Character.IntegrationTests.Helpers;
using Character.IntegrationTests.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Character.IntegrationTests.Travel
{
    [TestClass]
    public class TravelControllerTests : WebHostTestBase
    {
        [TestMethod]
        public async Task User_can_move_its_active_character()
        {
            // Arrange
            AuthenticateWith(UserIdWithCharacter);
            var character = await Client.GetFromJsonAsync<CharacterDto>("/MyCharacter");

            // Act
            var result = await Client.PostAsync("/Travel", JsonContent.Create(new StartTravelRequest
            {
                CharacterId = character.Id,
                X = 11,
                Y = -5
            }));
            var newLocation = await result.Content.ReadObjectFromJsonAsync<CharacterLocationDto>();
            
            // Assert
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.IsNotNull(newLocation);
            Assert.AreEqual(character.Id, newLocation.CharacterId);
            Assert.AreEqual(11, newLocation.X);
            Assert.AreEqual(-5, newLocation.Y);
        }

        [TestMethod]
        public async Task User_cannot_move_someone_else_character()
        {
            // Arrange
            AuthenticateWith(UserIdWithCharacter);
            var character = await Client.GetFromJsonAsync<CharacterDto>("/MyCharacter");
            AuthenticateWith(UserIdWithoutCharacter);

            // Act
            var result = await Client.PostAsync("/Travel", JsonContent.Create(new StartTravelRequest
            {
                CharacterId = character.Id,
                X = 11,
                Y = -5
            }));

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }
    }
}

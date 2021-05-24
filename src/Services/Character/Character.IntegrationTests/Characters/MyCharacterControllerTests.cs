using System.Net;
using System.Threading.Tasks;
using Character.Api.Application.Characters;
using Character.Api.Domain.Characters;
using Character.IntegrationTests.Helpers;
using Character.IntegrationTests.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Character.IntegrationTests.Characters
{
    [TestClass]
    public class MyCharacterControllerTests : WebHostTestBase
    {

        [TestMethod]
        public async Task User_can_get_its_character()
        {
            // Arrange
            AuthenticateWith(UserIdWithCharacter);

            // Act
            var result = await Client.GetAsync("/MyCharacter");
            var character = await result.Content.ReadObjectFromJsonAsync<CharacterDto>();
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(character);
            Assert.AreEqual("Hello", character.FirstName);
            Assert.AreEqual("Moon", character.LastName);
            Assert.AreEqual(SexType.Male, character.Sex);
        }

        [TestMethod]
        public async Task Error_401_is_thrown_if_api_is_requested_without_JWT_token()
        {
            // Act
            var result = await Client.GetAsync("/MyCharacter");
            
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [TestMethod]
        public async Task Error_404_is_thrown_if_user_requests_its_character_while_it_has_none()
        {
            // Arrange
            AuthenticateWith(UserIdWithoutCharacter);

            // Act
            var result = await Client.GetAsync("/MyCharacter");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
        }
    }
}

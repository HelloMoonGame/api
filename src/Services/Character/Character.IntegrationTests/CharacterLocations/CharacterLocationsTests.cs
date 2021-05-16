using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Character.Api.Application.CharacterLocations.GetCharacterLocation;
using Character.Api.Application.CharacterLocations.GetCharacterLocations;
using Character.Api.Application.CharacterLocations.SpawnCharacter;
using Character.Api.Application.Characters.CreateCharacter;
using Character.Api.Domain.CharacterLocations.Rules;
using Character.Api.Domain.Characters;
using Character.Api.GrpcServices;
using Character.IntegrationTests.SeedWork;
using CharacterApi;
using Common.Domain.SeedWork;
using Grpc.Core;
using Grpc.Core.Testing;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

        [TestMethod]
        public async Task User_can_get_the_location_of_its_current_character()
        {
            var userId = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);

            var character = await mediator.Send(new CreateCharacterCommand(
                userId,
                "FirstName",
                "LastName",
                SexType.Male));

            var spawnLocation = await mediator.Send(new SpawnCharacterCommand(character.Id));

            var characterLocation = await mediator.Send(new GetCharacterLocationQuery(character.Id));
            
            Assert.IsNotNull(characterLocation);
            Assert.AreEqual(character.Id, characterLocation.CharacterId);
            Assert.AreEqual(spawnLocation.X, characterLocation.X);
            Assert.AreEqual(spawnLocation.Y, characterLocation.Y);
        }

        [TestMethod]
        public async Task Users_receive_a_list_of_locations_of_all_online_characters()
        {
            var userId = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);

            var character = await mediator.Send(new CreateCharacterCommand(
                userId,
                "FirstName",
                "LastName",
                SexType.Male));
            var character2 = await mediator.Send(new CreateCharacterCommand(
                userId2,
                "FirstName",
                "LastName",
                SexType.Male));

            await mediator.Send(new SpawnCharacterCommand(character.Id));
            await mediator.Send(new SpawnCharacterCommand(character2.Id));

            var characterLocations = (await mediator.Send(new GetCharacterLocationsQuery(new [] { character.Id, character2.Id }))).ToList();
            
            Assert.IsNotNull(characterLocations);
            Assert.AreEqual(2, characterLocations.Count);
            CollectionAssert.AreEquivalent(new[] { character.Id, character2.Id }, 
                characterLocations.Select(l => l.CharacterId).ToArray());
        }

        [TestMethod]
        public async Task Users_get_informed_if_a_new_character_spawns()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);

            var character = await mediator.Send(new CreateCharacterCommand(
                userId,
                "FirstName",
                "LastName",
                SexType.Male));
            var character2 = await mediator.Send(new CreateCharacterCommand(
                userId2,
                "FirstName",
                "LastName",
                SexType.Male));

            await mediator.Send(new SpawnCharacterCommand(character.Id));
            
            var locationService = new LocationService(Services.GetService<ILogger<LocationService>>(),
                Services.GetService<IMediator>());

            var responseStream = new Mock<IServerStreamWriter<LocationUpdateResponse>>();
            var locationUpdates = new List<LocationUpdate>();
            responseStream.Setup(r => r.WriteAsync(It.IsAny<LocationUpdateResponse>()))
                .Callback<LocationUpdateResponse>((locationUpdateResponse) =>
                {
                    locationUpdates.AddRange(locationUpdateResponse.LocationUpdates);
                });
            
            var cancellationToken = new CancellationTokenSource();
            var context = TestServerCallContext.Create(null, null, new DateTime(),
                null, cancellationToken.Token, null, null,
                null, null, null, null);
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                })));
            context.UserState["__HttpContext"] = httpContext.Object;
            _ = locationService.Subscribe(new Empty(), responseStream.Object, context);
            locationUpdates.Clear(); // clear initial updates
            
            // Act
            await mediator.Send(new SpawnCharacterCommand(character2.Id), cancellationToken.Token);
            cancellationToken.Cancel();

            // Assert
            Assert.IsNotNull(locationUpdates);
            Assert.AreEqual(1, locationUpdates.Count);
            Assert.AreEqual(character2.Id.ToString(), locationUpdates[0].CharacterId);
        }
    }
}

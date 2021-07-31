using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Character.Api.Application.CharacterLocations.GetCharacterLocation;
using Character.Api.Application.CharacterLocations.GetCharacterLocations;
using Character.Api.Application.CharacterLocations.MoveCharacter;
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

namespace Character.IntegrationTests.Travel
{
    [TestClass]
    public class TravelTests : TestBase
    {
        [TestMethod]
        public async Task A_new_character_can_travel_to_any_location()
        {
            var userId = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);

            var character = await mediator.Send(new CreateCharacterCommand(
                userId,
                "FirstName",
                "LastName",
                SexType.Male));

            var characterLocation = await mediator.Send(new MoveCharacterCommand(character.Id, 10, -4));

            Assert.IsNotNull(characterLocation);
            Assert.AreEqual(character.Id, characterLocation.CharacterId);
            Assert.AreEqual(10, characterLocation.X);
            Assert.AreEqual(-4, characterLocation.Y);
        }
        
        [TestMethod]
        public async Task A_new_character_can_travel_twice()
        {
            var userId = Guid.NewGuid();

            var mediator = Services.GetService<IMediator>();
            Assert.IsNotNull(mediator);

            var character = await mediator.Send(new CreateCharacterCommand(
                userId,
                "FirstName",
                "LastName",
                SexType.Male));

            var characterLocation1 = await mediator.Send(new MoveCharacterCommand(character.Id, 10, -4));
            var characterLocation2 = await mediator.Send(new MoveCharacterCommand(character.Id, 11, -5));

            Assert.IsNotNull(characterLocation1);
            Assert.AreEqual(character.Id, characterLocation1.CharacterId);
            Assert.AreEqual(10, characterLocation1.X);
            Assert.AreEqual(-4, characterLocation1.Y);

            Assert.IsNotNull(characterLocation2);
            Assert.AreEqual(character.Id, characterLocation2.CharacterId);
            Assert.AreEqual(11, characterLocation2.X);
            Assert.AreEqual(-5, characterLocation2.Y);
        }

        [TestMethod]
        public async Task Users_get_informed_if_a_character_moves()
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

            await mediator.Send(new MoveCharacterCommand(character.Id, 10, 5));
            await mediator.Send(new MoveCharacterCommand(character2.Id, 10, 5));

            var locationService = new LocationService(Services.GetService<ILogger<LocationService>>(),
                Services.GetService<IMediator>());

            var responseStream = new Mock<IServerStreamWriter<LocationUpdateResponse>>();
            var locationUpdates = new List<LocationUpdate>();
            responseStream.Setup(r => r.WriteAsync(It.IsAny<LocationUpdateResponse>()))
                .Callback<LocationUpdateResponse>(locationUpdateResponse =>
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
            await mediator.Send(new MoveCharacterCommand(character2.Id, 9, 4), cancellationToken.Token);
            cancellationToken.Cancel();

            // Assert
            Assert.IsNotNull(locationUpdates);
            Assert.AreEqual(1, locationUpdates.Count);
            Assert.AreEqual(character2.Id.ToString(), locationUpdates[0].CharacterId);
            Assert.AreEqual(9, locationUpdates[0].X);
            Assert.AreEqual(4, locationUpdates[0].Y);
        }
    }
}
